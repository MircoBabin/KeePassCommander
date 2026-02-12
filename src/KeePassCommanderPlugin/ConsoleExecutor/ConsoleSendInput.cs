using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace ConsoleExecutor
{
    class ConsoleSendInput
    {
        private static class NativeMethods
        {
            public const int VK_RETURN = 0x0D; //actually the ENTER key

            public const int VK_SHIFT = 0x10;
            public const int VK_LSHIFT = 0xA0;
            public const int VK_RSHIFT = 0xA1;

            public const int VK_CONTROL = 0x11;
            public const int VK_LCONTROL = 0xA2;
            public const int VK_RCONTROL = 0xA3;

            public const int VK_MENU = 0x12; // actually the ALT key
            public const int VK_LMENU = 0xA4;
            public const int VK_RMENU = 0xA5;

            public const int VK_CAPITAL = 0x14; // actually the CAPS LOCK key

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern short GetAsyncKeyState(int vkKeyCode);

            [StructLayout(LayoutKind.Sequential)]
            public struct KeyboardInput
            {
                public ushort wVk;
                public ushort wScan;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MouseInput
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public uint dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct HardwareInput
            {
                public uint uMsg;
                public ushort wParamL;
                public ushort wParamH;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct InputUnion
            {
                [FieldOffset(0)] public MouseInput mi;
                [FieldOffset(0)] public KeyboardInput ki;
                [FieldOffset(0)] public HardwareInput hi;
            }

            public struct Input
            {
                public int type;
                public InputUnion u;
            }

            [Flags]
            public enum InputType
            {
                Mouse = 0,
                Keyboard = 1,
                Hardware = 2
            }

            [Flags]
            public enum KeyEventF
            {
                ExtendedKey = 0x0001,
                KeyUp = 0x0002,
                Unicode = 0x0004,
                Scancode = 0x0008
            }

            [Flags]
            public enum MouseEventF
            {
                Absolute = 0x8000,
                HWheel = 0x01000,
                Move = 0x0001,
                MoveNoCoalesce = 0x2000,
                LeftDown = 0x0002,
                LeftUp = 0x0004,
                RightDown = 0x0008,
                RightUp = 0x0010,
                MiddleDown = 0x0020,
                MiddleUp = 0x0040,
                VirtualDesk = 0x4000,
                Wheel = 0x0800,
                XDown = 0x0080,
                XUp = 0x0100
            }

            [DllImport("user32.dll", SetLastError = true)]
            public static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

            public static void SendKeyboardInputs(List<KeyboardInput> kbInputs, int milliSecondsDelayAfterSending)
            {
                if (kbInputs.Count == 0) return;

                Input[] inputs = new Input[kbInputs.Count];

                for (int i = 0; i < kbInputs.Count; i++)
                {
                    var input = new Input
                    {
                        type = (int)InputType.Keyboard,
                        u = new InputUnion
                        {
                            ki = kbInputs[i]
                        }
                    };

                    input.u.ki.time = 0;
                    input.u.ki.dwExtraInfo = IntPtr.Zero;

                    if ((input.u.ki.dwFlags & (uint)KeyEventF.Unicode) > 0)
                    {
                        // The wVk parameter must be zero.
                        // The wScan parameter specifies a Unicode character which is to be sent to the foreground application.
                        // This flag can only be combined with the KEYEVENTF_KEYUP flag.
                        input.u.ki.wVk = 0;

                        if ((input.u.ki.dwFlags & (uint)KeyEventF.KeyUp) > 0)
                            input.u.ki.dwFlags = (uint)(KeyEventF.Unicode | KeyEventF.KeyUp);
                        else
                            input.u.ki.dwFlags = (uint)(KeyEventF.Unicode);
                    }
                    else if ((input.u.ki.dwFlags & (uint)KeyEventF.Scancode) > 0)
                    {
                        // wScan identifies the key and wVk is ignored.
                        input.u.ki.wVk = 0;

                        // KEYEVENTF_EXTENDEDKEY - If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).
                    } 

                    inputs[i] = input;
                }

                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

                if (milliSecondsDelayAfterSending >= 0)
                    Thread.Sleep(milliSecondsDelayAfterSending);
            }

            public static bool Is_VirtualKey_Pressed(short vkKeyCode)
            {
                return (GetAsyncKeyState(vkKeyCode) & 0x8000) > 0;
            }

            public static List<KeyboardInput> Press_VirtualKey(short vkKeyCode)
            {
                var inputs = new List<KeyboardInput>
                {
                    new KeyboardInput
                    {
                        wVk = (ushort)vkKeyCode,
                        wScan = 0,
                        dwFlags = 0,
                    }
                };

                return inputs;
            }

            public static List<KeyboardInput> Release_VirtualKey(short vkKeyCode)
            {
                var inputs = new List<KeyboardInput>
                {
                    new KeyboardInput
                    {
                        wVk = (ushort)vkKeyCode,
                        wScan = 0,
                        dwFlags = (uint)(KeyEventF.KeyUp),
                    }
                };

                return inputs;
            }

            public static List<KeyboardInput> ToggleOff_CAPS_LOCK()
            {
                const short vkKeyCode = VK_CAPITAL;
                var isOn = (GetAsyncKeyState(vkKeyCode) & 0x01) > 0;

                var inputs = new List<KeyboardInput>();
                if (isOn)
                {
                    inputs.AddRange(Press_VirtualKey(vkKeyCode));
                    inputs.AddRange(Release_VirtualKey(vkKeyCode));
                }

                return inputs;
            }

            public static List<KeyboardInput> Release_Shift_Ctrl_Alt_Keys()
            {
                var inputs = new List<KeyboardInput>();

                inputs.AddRange(Press_VirtualKey(VK_LSHIFT));
                inputs.AddRange(Release_VirtualKey(VK_LSHIFT));

                inputs.AddRange(Press_VirtualKey(VK_RSHIFT));
                inputs.AddRange(Release_VirtualKey(VK_RSHIFT));

                inputs.AddRange(Press_VirtualKey(VK_LCONTROL));
                inputs.AddRange(Release_VirtualKey(VK_LCONTROL));

                inputs.AddRange(Press_VirtualKey(VK_RCONTROL));
                inputs.AddRange(Release_VirtualKey(VK_RCONTROL));

                inputs.AddRange(Press_VirtualKey(VK_LMENU));
                inputs.AddRange(Release_VirtualKey(VK_LMENU));

                inputs.AddRange(Press_VirtualKey(VK_RMENU));
                inputs.AddRange(Release_VirtualKey(VK_RMENU));

                return inputs;
            }


            public static List<KeyboardInput> Keypress_VirtualKey(short vkKeyScan)
            {
                var inputs = new List<KeyboardInput>();

                short vkKeyCode = (short) (vkKeyScan & 0xff);
                if (vkKeyCode == 0xff) return inputs;

                var vkCodeShift = (vkKeyScan & 0x100) > 0;
                var vkCodeCtrl = (vkKeyScan & 0x200) > 0;
                var vkCodeAlt = (vkKeyScan & 0x400) > 0;

                if (vkCodeShift)
                {
                    inputs.AddRange(Press_VirtualKey(VK_LSHIFT));
                    inputs.AddRange(Release_VirtualKey(VK_RSHIFT));
                }
                else
                {
                    inputs.AddRange(Release_VirtualKey(VK_LSHIFT));
                    inputs.AddRange(Release_VirtualKey(VK_RSHIFT));
                }

                if (vkCodeCtrl)
                {
                    inputs.AddRange(Press_VirtualKey(VK_LCONTROL));
                    inputs.AddRange(Release_VirtualKey(VK_RCONTROL));
                }
                else
                {
                    inputs.AddRange(Release_VirtualKey(VK_LCONTROL));
                    inputs.AddRange(Release_VirtualKey(VK_RCONTROL));
                }

                if (vkCodeAlt)
                {
                    inputs.AddRange(Press_VirtualKey(VK_LMENU));
                    inputs.AddRange(Release_VirtualKey(VK_RMENU));
                }
                else
                {
                    inputs.AddRange(Release_VirtualKey(VK_LMENU));
                    inputs.AddRange(Release_VirtualKey(VK_RMENU));
                }

                inputs.AddRange(Press_VirtualKey(vkKeyCode));
                inputs.AddRange(Release_VirtualKey(vkKeyCode));

                return inputs;
            }

            public static List<KeyboardInput> Keypress_UnicodeChar(char[] ch)
            {
                var inputs = new List<KeyboardInput>();

                if (ch.Length == 0) return inputs;
                if (char.IsLowSurrogate(ch[0])) return inputs;

                if (char.IsHighSurrogate(ch[0]))
                {
                    if (ch.Length != 2) return inputs;
                    if (!char.IsLowSurrogate(ch[1])) return inputs;

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[0],
                            dwFlags = (uint)(KeyEventF.Unicode),
                        });

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[1],
                            dwFlags = (uint)(KeyEventF.Unicode),
                        });

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[0],
                            dwFlags = (uint)(KeyEventF.Unicode | KeyEventF.KeyUp),
                        });

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[1],
                            dwFlags = (uint)(KeyEventF.Unicode | KeyEventF.KeyUp),
                        });
                }
                else
                {
                    if (ch.Length != 1) return inputs;

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[0],
                            dwFlags = (uint)(KeyEventF.Unicode),
                        });

                    inputs.Add(
                        new KeyboardInput
                        {
                            wScan = ch[0],
                            dwFlags = (uint)(KeyEventF.Unicode | KeyEventF.KeyUp),
                        });
                }

                return inputs;
            }
        }

        public static void TypeInText(string text, bool appendENTERKey, int milliSecondsDelayBetweenKeyPresses)
        {
            var inputs = new List<NativeMethods.KeyboardInput>();

            inputs.Clear();
            inputs.AddRange(NativeMethods.Release_Shift_Ctrl_Alt_Keys());
            inputs.AddRange(NativeMethods.ToggleOff_CAPS_LOCK());
            NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);

            // Just in case toggle off a second time, should the first have failed.
            inputs.Clear();
            inputs.AddRange(NativeMethods.ToggleOff_CAPS_LOCK());
            NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);

            if (text != null)
            {
                int i = 0;
                while (i < text.Length)
                {
                    char[] ch = String_GetCharAt(text, i);
                    i += ch.Length;

                    inputs.Clear();
                    inputs.AddRange(NativeMethods.Keypress_UnicodeChar(ch));
                    NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
                }
            }

            if (appendENTERKey)
            {
                inputs.Clear();
                inputs.AddRange(NativeMethods.Keypress_VirtualKey(NativeMethods.VK_RETURN));
                NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
            }

            inputs.Clear();
            inputs.AddRange(NativeMethods.Release_Shift_Ctrl_Alt_Keys());
            NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
        }

        public static void TypeInText(SecureString text, bool appendENTERKey, int milliSecondsDelayBetweenKeyPresses)
        {
            var inputs = new List<NativeMethods.KeyboardInput>();
            inputs.AddRange(NativeMethods.ToggleOff_CAPS_LOCK());
            inputs.AddRange(NativeMethods.Release_Shift_Ctrl_Alt_Keys());
            NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);

            if (text != null)
            {
                int i = 0;
                while (i < text.Length)
                {
                    char[] ch = SecureString_GetCharAt(text, i);
                    i += ch.Length;

                    inputs.Clear();
                    inputs.AddRange(NativeMethods.Keypress_UnicodeChar(ch));
                    NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
                }
            }

            if (appendENTERKey)
            {
                inputs.Clear();
                inputs.AddRange(NativeMethods.Keypress_VirtualKey(NativeMethods.VK_RETURN));
                NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
            }

            inputs.Clear();
            inputs.AddRange(NativeMethods.Release_Shift_Ctrl_Alt_Keys());
            NativeMethods.SendKeyboardInputs(inputs, milliSecondsDelayBetweenKeyPresses);
        }

        private static char[] String_GetCharAt(string source, int index)
        {
            if (index < 0 || index >= source.Length)
                throw new ArgumentException("index should be between 0 and source.length-1");

            char ch = source[index];

            if (char.IsHighSurrogate(ch))
            {
                // UTF-16 High part of surrogate pair.
                if (index == (source.Length - 1)) return new char[1] { ch };
                index++;

                return new char[2] { ch, source[index] };
            }

            if (char.IsLowSurrogate(ch))
            {
                // UTF-16 Low part of surrogate pair.
                if (index == 0) return new char[1] { ch };
                index--;

                return new char[2] { source[index], ch };
            }

            // UTF-16 no surrogate, one character
            return new char[1] { ch };
        }

        private static char[] SecureString_GetCharAt(SecureString source, int index)
        {
            if (index < 0 || index >= source.Length)
                throw new ArgumentException("index should be between 0 and source.length-1");

            IntPtr pointer = IntPtr.Zero;
            try
            {
                pointer = Marshal.SecureStringToGlobalAllocUnicode(source);

                char[] onechar = new char[1];
                Marshal.Copy(new IntPtr(pointer.ToInt64() + (2 * index)), onechar, 0, 1);

                if (char.IsHighSurrogate(onechar[0]))
                {
                    // UTF-16 High part of surrogate pair.
                    if (index == (source.Length-1)) return onechar;
                    index++;

                    char[] surrogate = new char[2] { onechar[0], '\0' };
                    Marshal.Copy(new IntPtr(pointer.ToInt64() + (2 * index)), surrogate, 1, 1);

                    return surrogate;
                }

                if (char.IsLowSurrogate(onechar[0]))
                {
                    // UTF-16 Low part of surrogate pair.
                    if (index == 0) return onechar;
                    index--;

                    char[] surrogate = new char[2] { '\0', onechar[0] };
                    Marshal.Copy(new IntPtr(pointer.ToInt64() + (2 * index)), surrogate, 0, 1);

                    return surrogate;
                }

                // UTF-16 no surrogate, one character
                return onechar;
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(pointer);
                }
            }
        }
    }
}
