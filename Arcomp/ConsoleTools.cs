using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Arcomp {
    /// <summary> Very quick-and-dirty console write helper methods. </summary>
    internal static class ConsoleTools {
        private static readonly object Locker = new object();
        private static readonly Stack<ConsoleColor> PushedForegroundColor = new Stack<ConsoleColor>();
        private static int _indentLevel;
        private const int IndentLength = 2;
        private static bool _openedWrite;

        public static void Info(string message) {
            Contract.Requires(!string.IsNullOrEmpty(message));

            lock (Locker) {
                Console.WriteLine(message);
            }

        }

        public static void Error(string message) {
            Contract.Requires(!string.IsNullOrEmpty(message));

            lock (Locker) {
                PushForeground(ConsoleColor.Red);
                InternalWrite("Error: ");
                PopForegroundOnce();
                InternalWriteLine(message);
            }

        }

        public static void Exception(Exception e) {
            Contract.Requires(e != null);

            lock (Locker) {
                PushForeground(ConsoleColor.Red);
                InternalWrite("Program halt: ");
                PopForegroundOnce();
                InternalWriteLine(e.Message);
                PushForeground(ConsoleColor.DarkRed);
                InternalWriteLine(e.StackTrace);
                PopForegroundAll();
            }
        }

        public static void Indent() {
            lock (Locker) {
                _indentLevel++;
            }
        }

        public static void Unindent() {
            lock (Locker) {
                if (_indentLevel > 0) {
                    _indentLevel--;
                }
            }
        }

        public static void UnindentToZero() {
            lock (Locker) {
                _indentLevel = 0;
            }
        }

        public static void Write(string message) {
            Contract.Requires(message != null);

            lock (Locker) {
                InternalWrite(message);
            }
        }

        public static void WriteLine(string message = "") {
            Contract.Requires(message != null);

            lock (Locker) {
                InternalWriteLine(message);
            }
        }


        private static void InternalWrite(string message) {
            Contract.Requires(message != null);

            message = message.Replace(Environment.NewLine, IndentPrefix() + Environment.NewLine);
            if (!_openedWrite) {
                message = IndentPrefix() + message;
                _openedWrite = true;
            }

            Console.Write(message);
        }

        private static void InternalWriteLine(string message) {
            Contract.Requires(message != null);

            InternalWrite(message);
            Console.Write(Environment.NewLine);
            _openedWrite = false;
        }

        private static string IndentPrefix() {
            lock (Locker) {
                string indentString = " ".Repeat(IndentLength);
                return indentString.Repeat(_indentLevel);
            }
        }

        private static void PushForeground(ConsoleColor foreground) {
            lock (Locker) {
                PushedForegroundColor.Push(Console.ForegroundColor);
                Console.ForegroundColor = foreground;
            }
        }

        /// <exception cref="InvalidOperationException">PushForeground/RestoreForeground imbalance.</exception>
        private static void PopForegroundOnce() {
            lock (Locker) {
                if (PushedForegroundColor.Count > 0) {
                    Console.ForegroundColor = PushedForegroundColor.Pop();
                } else {
                    throw new InvalidOperationException("PushForeground/RestoreForeground imbalance.");
                }
            }
        }

        private static void PopForegroundAll() {
            lock (Locker) {
                var first = Console.ForegroundColor;
                while (PushedForegroundColor.Count > 0) {
                    first = PushedForegroundColor.Pop();
                }

                Console.ForegroundColor = first;
            }
        }
    }
}
