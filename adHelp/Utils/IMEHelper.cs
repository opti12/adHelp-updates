using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace adHelp.Utils
{
    /// <summary>
    /// IME(입력기) 관리 유틸리티 클래스
    /// TextBox에서 영문 입력만 가능하도록 IME를 제어합니다.
    /// </summary>
    public static class IMEHelper
    {
        #region Windows API

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);
        
        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        
        [DllImport("imm32.dll")]
        private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

        #endregion

        /// <summary>
        /// TextBox의 IME를 영문으로 고정
        /// ImeMode 설정과 Enter 이벤트 IME 비활성화를 함께 적용합니다.
        /// </summary>
        /// <param name="textBox">IME를 고정할 TextBox</param>
        public static void SetEnglishOnly(TextBox textBox)
        {
            if (textBox == null) return;

            try
            {


                // ImeMode 설정 (기본 방어선)
                textBox.ImeMode = ImeMode.Disable;

                // Enter 이벤트에 IME 비활성화 연결
                textBox.Enter += (sender, e) => {
                    DisableIME(textBox);
                };

                // Leave 이벤트에도 연결 (다른 컨트롤로 이동 시에도 적용)
                textBox.Leave += (sender, e) => {
                    DisableIME(textBox);
                };

                // 현재 포커스가 있다면 즉시 적용
                if (textBox.Focused)
                {
                    DisableIME(textBox);
                }


            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"IMEHelper: SetEnglishOnly 오류 (TextBox: {textBox?.Name}): {ex.Message}");
            }
        }

        /// <summary>
        /// 여러 TextBox에 한번에 영문 고정 적용
        /// </summary>
        /// <param name="textBoxes">IME를 고정할 TextBox 배열</param>
        public static void SetEnglishOnly(params TextBox[] textBoxes)
        {
            if (textBoxes == null || textBoxes.Length == 0) return;



            foreach (var textBox in textBoxes)
            {
                SetEnglishOnly(textBox);
            }


        }

        /// <summary>
        /// Windows API를 사용하여 IME 비활성화
        /// </summary>
        /// <param name="textBox">IME를 비활성화할 TextBox</param>
        private static void DisableIME(TextBox textBox)
        {
            if (textBox == null || !textBox.IsHandleCreated) return;

            try
            {
                IntPtr hIMC = ImmGetContext(textBox.Handle);
                if (hIMC != IntPtr.Zero)
                {
                    ImmSetOpenStatus(hIMC, false);
                    ImmReleaseContext(textBox.Handle, hIMC);

                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"IMEHelper: DisableIME 오류 (TextBox: {textBox?.Name}): {ex.Message}");
            }
        }

        /// <summary>
        /// TextBox의 IME 상태 확인 (디버깅용)
        /// </summary>
        /// <param name="textBox">상태를 확인할 TextBox</param>
        /// <returns>IME 활성화 여부</returns>
        public static bool IsIMEEnabled(TextBox textBox)
        {
            if (textBox == null || !textBox.IsHandleCreated) return false;

            try
            {
                IntPtr hIMC = ImmGetContext(textBox.Handle);
                if (hIMC != IntPtr.Zero)
                {
                    // IME 상태를 확인하는 추가 API 호출이 필요하지만
                    // 여기서는 단순히 ImeMode 속성을 확인
                    ImmReleaseContext(textBox.Handle, hIMC);
                    return textBox.ImeMode != ImeMode.Disable;
                }
                return false;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"IMEHelper: IsIMEEnabled 오류 (TextBox: {textBox?.Name}): {ex.Message}");
                return false;
            }
        }
    }
}
