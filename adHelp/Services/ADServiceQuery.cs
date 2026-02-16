using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using adHelp.Models;
using adHelp.Utils;

namespace adHelp.Services
{
    /// <summary>
    /// Active Directory 고급 쿼리 및 상세 조회 서비스
    /// </summary>
    public partial class ADService
    {
        /// <summary>
        /// 사용자 검색 (부분 일치)
        /// </summary>
        /// <param name="searchTerm">검색어</param>
        /// <param name="maxResults">최대 결과 수</param>
        /// <returns>사용자 정보 리스트</returns>
        public List<UserInfo> SearchUsers(string searchTerm, int maxResults = 50)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(searchTerm))
                throw new ArgumentException("검색어는 필수입니다.");

            try
            {
                // 여러 필드에서 검색하는 LDAP 필터
                string filter = $"(&(objectCategory=person)(objectClass=user)" +
                              $"(|(sAMAccountName=*{searchTerm}*)" +
                              $"(displayName=*{searchTerm}*)" +
                              $"(mail=*{searchTerm}*)" +
                              $"(cn=*{searchTerm}*)))";

                _searcher.Filter = filter;
                _searcher.SizeLimit = maxResults;
                
                // 가져올 속성 지정
                _searcher.PropertiesToLoad.Clear();
                AddPropertiesToLoad();

                // 검색 실행
                SearchResultCollection results = _searcher.FindAll();
                
                var userList = new List<UserInfo>();
                
                foreach (SearchResult result in results)
                {
                    try
                    {
                        UserInfo userInfo = MapSearchResultToUserInfo(result);
                        if (userInfo != null)
                        {
                            userList.Add(userInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 사용자 매핑 실패는 로그만 남기고 계속 진행
                        System.Diagnostics.Debug.WriteLine($"사용자 매핑 실패: {ex.Message}");
                    }
                }

                return userList.OrderBy(u => u.UserId).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"사용자 검색 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사용자 상세 정보 조회 (Get-ADUser -Properties * 결과와 유사)
        /// </summary>
        /// <param name="userId">사용자 ID (SAM Account Name)</param>
        /// <returns>사용자 상세 정보 객체</returns>
        public UserDetailInfo GetUserDetailInfo(string userId)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("사용자 ID는 필수입니다.");

            try
            {
                // LDAP 필터 설정
                _searcher.Filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={userId}))";
                
                // 모든 속성 가져오기
                _searcher.PropertiesToLoad.Clear();
                // 주요 속성들을 명시적으로 추가
                AddAllPropertiesToLoad();

                // 검색 실행
                SearchResult result = _searcher.FindOne();
                
                if (result == null)
                {
                    return null; // 사용자 없음
                }

                // UserDetailInfo 객체 생성 및 데이터 매핑
                var userDetailInfo = new UserDetailInfo();
                
                // 기본 사용자 정보 매핑
                userDetailInfo.BasicInfo = MapSearchResultToUserInfo(result);
                
                // 모든 속성 매핑
                foreach (string propertyName in result.Properties.PropertyNames)
                {
                    try
                    {
                        var propertyCollection = result.Properties[propertyName];
                        if (propertyCollection != null && propertyCollection.Count > 0)
                        {
                            // 모든 값을 배열로 저장
                            if (propertyCollection.Count == 1)
                            {
                                userDetailInfo.AllProperties[propertyName] = propertyCollection[0];
                            }
                            else
                            {
                                var values = new object[propertyCollection.Count];
                                propertyCollection.CopyTo(values, 0);
                                userDetailInfo.AllProperties[propertyName] = values;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 개별 속성 처리 실패는 로그만 남기고 계속
                        System.Diagnostics.Debug.WriteLine($"속성 '{propertyName}' 처리 실패: {ex.Message}");
                    }
                }

                return userDetailInfo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"사용자 상세 정보 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사용자의 그룹 멤버십 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>그룹 목록</returns>
        public string[] GetUserGroups(string userId)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            try
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(_principalContext, userId))
                {
                    if (user == null)
                        return new string[0];

                    var groups = new List<string>();
                    
                    PrincipalSearchResult<Principal> groupResults = user.GetGroups();
                    foreach (Principal group in groupResults)
                    {
                        groups.Add(group.Name);
                    }

                    return groups.OrderBy(g => g).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"그룹 정보 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 그룹 기본 정보 조회
        /// </summary>
        /// <param name="groupName">그룹명</param>
        /// <returns>그룹 정보 딕셔너리</returns>
        public Dictionary<string, object> GetGroupInfo(string groupName)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("그룹명은 필수입니다.");

            try
            {
                // LDAP 필터 설정 - 그룹 검색
                _searcher.Filter = $"(&(objectCategory=group)(objectClass=group)(name={groupName}))";
                
                // 그룹 속성들 추가
                _searcher.PropertiesToLoad.Clear();
                AddGroupPropertiesToLoad();

                // 검색 실행
                SearchResult result = _searcher.FindOne();
                
                if (result == null)
                {
                    return null; // 그룹 없음
                }

                var groupInfo = new Dictionary<string, object>();
                
                // 그룹 정보 매핑
                foreach (string propertyName in result.Properties.PropertyNames)
                {
                    try
                    {
                        var propertyCollection = result.Properties[propertyName];
                        if (propertyCollection != null && propertyCollection.Count > 0)
                        {
                            if (propertyCollection.Count == 1)
                            {
                                groupInfo[propertyName] = propertyCollection[0];
                            }
                            else
                            {
                                var values = new object[propertyCollection.Count];
                                propertyCollection.CopyTo(values, 0);
                                groupInfo[propertyName] = values;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"그룹 속성 '{propertyName}' 처리 실패: {ex.Message}");
                    }
                }

                return groupInfo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"그룹 정보 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 그룹 구성원 조회 (개선된 디버깅 및 다중 검색 방법 지원)
        /// </summary>
        /// <param name="groupName">그룹명</param>
        /// <returns>구성원 정보 리스트</returns>
        public List<GroupMemberInfo> GetGroupMembers(string groupName)
        {
            if (!_isConnected)
                throw new InvalidOperationException("AD에 연결되어 있지 않습니다.");

            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("그룹명은 필수입니다.");

            SimpleLogger.Log($"GetGroupMembers 시작: 그룹명='{groupName}'");
            
            try
            {
                GroupPrincipal group = null;
                
                // 1단계: 그룹명으로 검색
                SimpleLogger.Log($"1단계: 그룹명으로 검색 시도: '{groupName}'");
                group = GroupPrincipal.FindByIdentity(_principalContext, groupName);
                
                if (group == null)
                {
                    // 2단계: SAM Account Name으로 검색
                    SimpleLogger.Log($"2단계: SAM Account Name으로 검색 시도: '{groupName}'");
                    group = GroupPrincipal.FindByIdentity(_principalContext, IdentityType.SamAccountName, groupName);
                }
                
                if (group == null)
                {
                    // 3단계: Distinguished Name으로 검색 (CN= 접두사 추가)
                    string cnGroupName = $"CN={groupName}";
                    SimpleLogger.Log($"3단계: CN 접두사로 검색 시도: '{cnGroupName}'");
                    group = GroupPrincipal.FindByIdentity(_principalContext, IdentityType.DistinguishedName, cnGroupName);
                }
                
                if (group == null)
                {
                    SimpleLogger.Log($"오류: 그룹 '{groupName}'을 찾을 수 없습니다.");
                    return new List<GroupMemberInfo>();
                }

                SimpleLogger.Log($"그룹 발견: Name='{group.Name}', SAM='{group.SamAccountName}', DN='{group.DistinguishedName}'");
                
                using (group)
                {
                    var members = new List<GroupMemberInfo>();
                    
                    SimpleLogger.Log("GetMembers() 호출 시작...");
                    PrincipalSearchResult<Principal> memberResults = group.GetMembers();
                    
                    int memberCount = 0;
                    foreach (Principal member in memberResults)
                    {
                        memberCount++;
                        SimpleLogger.Log($"구성원 {memberCount}: Name='{member?.Name}', SAM='{member?.SamAccountName}', Type='{member?.GetType().Name}'");
                        
                        try
                        {
                            string displayName = member.DisplayName ?? member.Name ?? "-";
                            string samAccountName = member.SamAccountName ?? "-";
                            string memberType = member is UserPrincipal ? "사용자" : member is GroupPrincipal ? "그룹" : "기타";
                            string distinguishedName = member.DistinguishedName ?? "-";
                            string email = "-";
                            string enabled = "-";
                            
                            // 사용자인 경우 추가 정보
                            if (member is UserPrincipal userMember)
                            {
                                try
                                {
                                    email = userMember.EmailAddress ?? "-";
                                    enabled = userMember.Enabled?.ToString() ?? "알 수 없음";
                                }
                                catch (Exception emailEx)
                                {
                                    SimpleLogger.Log($"사용자 '{member.SamAccountName}' 이메일/활성화 정보 조회 실패: {emailEx.Message}");
                                    email = "-";
                                    enabled = "알 수 없음";
                                }
                            }
                            
                            var memberInfo = new GroupMemberInfo(displayName, samAccountName, memberType, email, enabled, distinguishedName);
                            members.Add(memberInfo);
                            
                            SimpleLogger.Log($"구성원 추가 완료: {memberInfo.DisplayName} ({memberInfo.SAMAccountName})");
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.Log($"구성원 '{member?.Name}' 처리 실패: {ex.Message}");
                            // 개별 구성원 처리 실패는 로그만 남기고 계속
                        }
                    }

                    SimpleLogger.Log($"GetGroupMembers 완료: 총 {members.Count}명의 구성원 조회됨");
                    return members.OrderBy(m => m.DisplayName).ToList();
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"GetGroupMembers 예외 발생: {ex.Message}");
                SimpleLogger.Log($"Stack Trace: {ex.StackTrace}");
                throw new InvalidOperationException($"그룹 구성원 조회 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 그룹 속성들을 PropertiesToLoad에 추가
        /// </summary>
        private void AddGroupPropertiesToLoad()
        {
            string[] groupProperties = {
                "name", "displayName", "cn", "description", "distinguishedName",
                "objectGUID", "objectSid", "whenCreated", "whenChanged",
                "groupType", "groupScope", "managedBy", "member", "memberOf",
                "mail", "info", "notes", "adminDescription",
                "objectClass", "objectCategory", "sAMAccountName"
            };

            foreach (string property in groupProperties)
            {
                _searcher.PropertiesToLoad.Add(property);
            }
        }

        /// <summary>
        /// 모든 AD 속성들을 추가 (상세 정보용)
        /// </summary>
        private void AddAllPropertiesToLoad()
        {
            // Get-ADUser -Properties *와 유사한 모든 속성 추가
            string[] allProperties = {
                // 기본 정보
                "sAMAccountName", "displayName", "cn", "name", "givenName", "sn", "surname",
                "userPrincipalName", "distinguishedName", "canonicalName", "description",
                
                // 계정 정보
                "userAccountControl", "accountExpires", "accountExpirationDate", "created", "modified",
                "whenCreated", "whenChanged", "createTimeStamp", "modifyTimeStamp", "enabled",
                "accountNotDelegated", "allowReversiblePasswordEncryption", "cannotChangePassword",
                "passwordExpired", "passwordNeverExpires", "passwordNotRequired", "smartcardLogonRequired",
                "trustedForDelegation", "trustedToAuthForDelegation", "useDESKeyOnly",
                
                // 로그인 정보
                "lastLogon", "lastLogonDate", "lastLogonTimestamp", "lastLogoff", "logonCount",
                "logonWorkstations", "badLogonCount", "badPasswordTime", "badPwdCount",
                "lastBadPasswordAttempt",
                
                // 비밀번호 정보
                "pwdLastSet", "passwordLastSet",
                
                // 잠금 정보
                "lockedOut", "lockoutTime", "accountLockoutTime",
                
                // 연락처 정보
                "mail", "emailAddress", "telephoneNumber", "officePhone", "homePhone", "mobilePhone",
                "fax", "homePage",
                
                // 조직 정보
                "company", "department", "departmentNumber", "title", "manager", "employeeID",
                "employeeNumber", "organization", "division",
                
                // 위치 정보
                "office", "physicalDeliveryOfficeName", "city", "l", "state", "st", "country",
                "co", "c", "countryCode", "postalCode", "streetAddress", "poBox",
                
                // 개인 정보
                "initials", "otherName",
                
                // 그룹 정보
                "memberOf", "primaryGroup", "primaryGroupID",
                
                // 시스템 정보
                "objectGUID", "objectSid", "sid", "sidHistory", "objectClass", "objectCategory",
                "instanceType", "uSNChanged", "uSNCreated",
                
                // 고급 정보
                "homeDirectory", "homeDrive", "scriptPath", "profilePath", "loginScript",
                "codePage", "doesNotRequirePreAuth",
                
                // 관리 정보
                "adminDescription", "adminDisplayName", "protectedFromAccidentalDeletion",
                "carLicense", "destinationIndicator",
                
                // 인증 정보
                "authenticationPolicy", "authenticationPolicySilo", "kerberosEncryptionType",
                "servicePrincipalNames", "userCertificate", "certificates",
                
                // Terminal Services 정보  
                "msTSExpireDate", "msTSLicenseVersion", "msTSLicenseVersion2", "msTSLicenseVersion3",
                "msTSManagingLS",
                
                // 기타
                "msDS-User-Account-Control-Computed", "mS-DS-ConsistencyGuid", "nTSecurityDescriptor",
                "sDRightsEffective", "principalsAllowedToDelegateToAccount", "compoundIdentitySupported",
                "dSCorePropagationData", "lastKnownParent", "isDeleted", "deleted", "mNSLogonAccount"
            };

            foreach (string property in allProperties)
            {
                _searcher.PropertiesToLoad.Add(property);
            }
        }
    }
}
