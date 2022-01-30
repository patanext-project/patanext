using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;

public static class Program
{
    public static void Main()
    {
        bool CheckProgram(string program)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = program,
                    RedirectStandardOutput = true
                });
                process.Kill();
            }
            catch (Win32Exception)
            {
                return false;
            }
    
            return true;
        }

        if (!CheckProgram("godot"))
            Error("Godot isn't set in the environment variables");
        else
        {
            Debug("Starting godot project");

            var process = Process.Start(new ProcessStartInfo("godot", "Godot/project/project.godot")
            {
            }); 
            process.WaitForExit();
        }

        void Debug(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("DEBUG: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(msg);
            Console.WriteLine();
            Console.ResetColor();
        }

        void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("ERROR: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(msg);
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}