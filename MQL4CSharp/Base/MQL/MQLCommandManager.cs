﻿/*
Copyright 2016 Jason Separovic

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using RGiesecke.DllExport;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using log4net;
using mqlsharp.Util;
using MQL4CSharp.Base.Enums;
using MQL4CSharp.Base.Exceptions;

namespace MQL4CSharp.Base.MQL
{
    public class MQLCommandManager
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(MQLCommandManager));

        private static MQLCommandManager commandManager;

        public static MQLCommandManager getInstance()
        {
            if (commandManager == null)
            {
                commandManager = new MQLCommandManager();
            }
            return commandManager;
        }

        private MQLCommand command;
        private List<Object> parameters;
        private Boolean commandWaiting;
        private Object response;
        private int error;

        private readonly object syncLock = new object();

        static char DELIMITER = (char)29;


        private MQLCommandManager()
        {
            this.commandWaiting = false;
            this.error = 0;
        }


        public void ExecCommand(MQLCommand command, List<Object> parameters)
        {
            this.command = command;
            this.parameters = parameters;
            this.commandWaiting = true;
            this.error = 0;
        }

        public bool IsCommandRunning()
        {
            return MQLCommandManager.getInstance().commandWaiting;
        }

        public Object GetCommandResult()
        {
            return response;
        }


        [DllExport("IsCommandWaiting", CallingConvention = CallingConvention.StdCall)]
        public static bool IsCommandWaiting()
        {
            try
            {
                return MQLCommandManager.getInstance().commandWaiting;
            }
            catch (Exception e)
            {
                LOG.Error(e);
                return false;
            }
        }


        [DllExport("GetCommandId", CallingConvention = CallingConvention.StdCall)]
        public static int GetCommandId()
        {
            try
            {
                MQLCommandManager commandManager = MQLCommandManager.getInstance();

                if (commandManager.commandWaiting)
                {
                    return (int)commandManager.command;
                }
                else
                {
                    LOG.Error(Error.ERROR_NO_COMMAND.ToString());
                    return -1;
                }
            }
            catch (Exception e)
            {
                LOG.Error(e);
                return -1;
            }
        }


        [DllExport("GetCommandName", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string GetCommandName()
        {
            try
            {
                MQLCommandManager commandManager = MQLCommandManager.getInstance();

                if (commandManager.commandWaiting)
                {
                    return commandManager.command.ToString();
                }
                else
                {
                    LOG.Error(Error.ERROR_NO_COMMAND.ToString());
                    return Error.ERROR_NO_COMMAND.ToString();
                }
            }
            catch (Exception e)
            {
                LOG.Error(e);
                return Error.ERROR_UNHANDLED.ToString();
            }
        }

        [DllExport("GetCommandParams", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static string GetCommandParams()
        {
            try
            {
                MQLCommandManager commandManager = MQLCommandManager.getInstance();

                if (commandManager.commandWaiting)
                {
                    List<Object> parameters = commandManager.parameters;

                    String returnCommand = "";
                    foreach (Object p in parameters)
                    {
                        Object param = p;
                        if (param is DateTime)
                        {
                            // Convert DateTime to MT4 String
                            param = DateUtil.ToMT4TimeString((DateTime) p);
                        }

                        if(returnCommand.Equals(""))
                        {
                            returnCommand = returnCommand + param;
                        }
                        else
                        {
                            returnCommand = returnCommand + DELIMITER + param;
                        }
                    }
                    return returnCommand;
                }
                else
                {
                    LOG.Error(Error.ERROR_NO_COMMAND.ToString());
                    return Error.ERROR_NO_COMMAND.ToString();
                }
            }
            catch(Exception e)
            {
                LOG.Error(e);
                return Error.ERROR_UNHANDLED.ToString();
            }
        }

        private void setCommandResponse(Object response, int errorCode)
        {
            try
            {
                //LOG.Debug(String.Format("SetCommandResponse({0},{1})", response, errorCode));
                this.response = response;
                this.error = errorCode;
                this.commandWaiting = false;
            }
            catch (Exception e)
            {
                LOG.Error(e);
            }
        }

        public void throwExceptionIfErrorResponse()
        {
            if (this.error > 0)
            {
                MQLExceptions.throwMQLException(error);
            }
        }

        [DllExport("SetBoolCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetBoolCommandResponse(bool response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }

        [DllExport("SetDoubleCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetDoubleCommandResponse(double response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }

        [DllExport("SetIntCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetIntCommandResponse(int response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }

        [DllExport("SetStringCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetStringCommandResponse([In, Out, MarshalAs(UnmanagedType.LPWStr)] string response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }

        [DllExport("SetVoidCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetVoidCommandResponse(int error)
        {
            commandManager.setCommandResponse(null, error);
        }

        [DllExport("SetLongCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetLongCommandResponse(long response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }

        [DllExport("SetDateTimeCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetDateTimeCommandResponse(Int64 response, int error)
        {
            commandManager.setCommandResponse(DateUtil.FromUnixTime(response), error);
        }

        [DllExport("SetEnumCommandResponse", CallingConvention = CallingConvention.StdCall)]
        public static void SetEnumCommandResponse(int response, int error)
        {
            commandManager.setCommandResponse(response, error);
        }
    }
}


