'    Copyright (c) 2011-2013 Felix Belzile
'    Official software website: http://getcoldturkey.com
'    Contact: felixbelzile@gmail.com  Web: http://felixbelzile.com

'    This file is part of Cold Turkey
'
'    Cold Turkey is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    Cold Turkey is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with Cold Turkey.  If not, see <http://www.gnu.org/licenses/>.

Imports System.ServiceProcess
Imports System.IO
Imports System.Security.Cryptography
Imports Microsoft.Win32
Imports CTService.IniFile
Imports System.Threading
Imports SladeHome.NetworkTime
Imports System.Text

Public Class Service1
    Inherits System.ServiceProcess.ServiceBase
    Friend WithEvents timer10sBlocker As System.Timers.Timer
    Dim mutex As Threading.Mutex
    Dim hostsPath As String = Environment.GetFolderPath(Environment.SpecialFolder.System) & "\drivers\etc\hosts"
    Dim configPath As String = Application.StartupPath & "\CTConfig\CTConfig.ini"
    Dim encryption As New Crypto3DES()
    Dim blockStatus As Integer = 0
    Dim minCounter As Integer = 0
    Dim configChanged As Integer = 0
    Dim i As Integer = 0
    Dim processList As System.Diagnostics.Process() = Nothing
    Dim iniProcessList As String = ""
    Dim temp As String = ""
    Dim oldHashHosts() As Byte
    Dim oldHashConfig() As Byte
    Dim time As Integer
    Dim lastTimeCheck As Date = DateTime.Now

#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()
        MyBase.CanHandleSessionChangeEvent = True

        ' This call is required by the Component Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call

    End Sub

    'UserService overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Protected Overloads Sub OnStop(ByVal e As System.EventArgs)
        MyBase.OnStop()

        ' Restore previous state
        ' No way to recover; already exiting

    End Sub

    ' The main entry point for the process
    <MTAThread()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New Service1}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  
    ' Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.timer10sBlocker = New System.Timers.Timer()
        CType(Me.timer10sBlocker, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'timer10sBlocker
        '
        Me.timer10sBlocker.Enabled = True
        Me.timer10sBlocker.Interval = 10000.0R
        '
        'Service1
        '
        Me.CanStop = False
        Me.ServiceName = "CTService"
        CType(Me.timer10sBlocker, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

#End Region

    Protected Overrides Sub OnStart(ByVal args() As String)

        Dim thisConfig As Byte()

        'Is the Cold Turkey Configuration Server up? If not, start it
        checkConfigServer()

        thisConfig = ASCIIEncoding.ASCII.GetBytes(File.ReadAllText(configPath))
        oldHashConfig = New MD5CryptoServiceProvider().ComputeHash(thisConfig)
        checkBlock()

    End Sub

    Private Sub timer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles timer10sBlocker.Elapsed

        Dim iniFile As New IniFile
        Dim thisConfig() As Byte
        Dim thisHashConfig() As Byte
        Dim bEqual As Boolean = False
        Dim k As Integer = 0
        Dim j As Integer = 0
        Dim needToUpdate As Boolean = False

        If blockStatus = 1 Then

            Dim thisHosts() As Byte
            Dim thisHashHosts() As Byte

            thisHosts = ASCIIEncoding.ASCII.GetBytes(File.ReadAllText(hostsPath))
            thisHashHosts = New MD5CryptoServiceProvider().ComputeHash(thisHosts)

            If configChanged = 0 Then
                If thisHashHosts.Length = oldHashHosts.Length Then
                    Do While (j < thisHashHosts.Length) AndAlso (thisHashHosts(j) = oldHashHosts(j))
                        j += 1
                    Loop
                    If j <> thisHashHosts.Length Then
                        startBlock()
                    Else
                        oldHashHosts = thisHashHosts
                    End If
                Else
                    startBlock()
                End If
            Else
                oldHashHosts = thisHashHosts
            End If

            Try
                iniFile.Load(configPath)
                iniProcessList = iniFile.GetKeyValue("main", "programsBlocked").Replace("""", "")
                iniProcessList = encryption.Decrypt3DES(iniProcessList)
                If StrComp("null", iniProcessList) <> 0 Then
                    processList = System.Diagnostics.Process.GetProcesses()
                    For Each proc In processList
                        Try
                            If (iniProcessList.Contains(proc.ProcessName & ".exe") Or iniProcessList.Contains(proc.ProcessName.Replace("64", "") & ".exe")) Then
                                proc.Kill()
                            End If
                        Catch ex As Exception
                            My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\error.log", ex.ToString, True)
                        End Try
                    Next
                End If
            Catch ex As Exception
                resetConfigFile()
            End Try
        End If

        'Has the config file changed?
        thisConfig = ASCIIEncoding.ASCII.GetBytes(File.ReadAllText(configPath))
        thisHashConfig = New MD5CryptoServiceProvider().ComputeHash(thisConfig)

        If configChanged = 0 Then
            If thisHashConfig.Length = oldHashConfig.Length Then
                Do While (k < thisHashConfig.Length) AndAlso (thisHashConfig(k) = oldHashConfig(k))
                    k += 1
                Loop
                If k <> thisHashConfig.Length Then
                    configChanged = 1
                    checkBlock()
                    oldHashConfig = thisHashConfig
                Else
                    oldHashConfig = thisHashConfig
                End If
            Else
                configChanged = 1
                checkBlock()
                oldHashConfig = thisHashConfig
            End If
        Else
            oldHashConfig = thisHashConfig
            configChanged = 0
        End If

        minCounter = minCounter + 1

        If minCounter = 6 Then
            minCounter = 0

            'Is the Cold Turkey Configuration Server up? If not, start it
            checkConfigServer()

            If (Integer.Parse(DateDiff(DateInterval.Minute, DateTime.Now, lastTimeCheck)) > 2) Then
                needToUpdate = False
            Else
                needToUpdate = True
            End If

            If ((DateTime.Now.Minute = 0 Or DateTime.Now.Minute = 30) Or needToUpdate) Then
                needToUpdate = False
                lastTimeCheck = DateTime.Now
                checkBlock()
            End If

        End If

    End Sub
    Private Sub checkConfigServer()

        processList = System.Diagnostics.Process.GetProcesses()
        For Each proc In processList
            Try
                temp = temp & proc.ProcessName
            Catch ex As Exception
            End Try
        Next
        If temp.Contains("CTConfigServer") = False Then
            Process.Start(Application.StartupPath & "\CTConfigServer.exe")
        End If

    End Sub
    Private Sub resetConfigFile()
        Dim iniFile As New IniFile
        Try
            My.Computer.FileSystem.WriteAllText(configPath, "", False)
            iniFile.Load(configPath)
            iniFile.AddSection("main")
            iniFile.SetKeyValue("main", "websitesBlocked", """" & encryption.Encrypt3DES("null") & """")
            iniFile.SetKeyValue("main", "programsBlocked", """" & encryption.Encrypt3DES("null") & """")
            iniFile.SetKeyValue("main", "blockedTimes", """" & encryption.Encrypt3DES("0,0") & """")
            iniFile.Save(configPath)
        Catch ee As Exception
            My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\error.log", ee.ToString, True)
            stopBlock()
            End
        End Try
    End Sub

    Private Sub checkBlock()

        Dim ntc As NetworkTimeClient
        Dim ntcTime, epoch As DateTime
        Dim iniFile As New IniFile
        Dim iniTime() As String
        Dim timeFail As Integer = 0

        'Get NTP time
        Try
            ntc = New NetworkTimeClient("pool.ntp.org")
            ntcTime = ntc.Time
            epoch = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            time = (ntcTime - epoch).TotalSeconds
            timeFail = 0
        Catch ee As Exception
            timeFail = timeFail + 1
            If timeFail > 9 Then
                My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\error.log", ee.ToString, True)
                stopBlock()
                End
            End If
        End Try

        'Am I currently supposed to block?
        Try
            iniFile.Load(configPath)
            iniTime = Split(encryption.Decrypt3DES(iniFile.GetKeyValue("main", "blockedTimes").Replace("""", "")), ",")

            i = 0
            For Each timeString As String In iniTime
                If timeString.Length <> 0 Then
                    If time > Integer.Parse(timeString) Then
                        i = i + 1
                    End If
                End If
            Next

            'If current time crosses timestamp(s) odd number times, we are blocked
            If i Mod 2 <> 0 Then
                i = 0
                blockStatus = 1
                startBlock()
            Else
                i = 0
                blockStatus = 0
                stopBlock()
            End If
        Catch ee As Exception
            My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\error.log", ee.ToString, True)
            stopBlock()
            End
        End Try
    End Sub

    Private Sub startBlock()

        Dim iniFile As New IniFile
        Dim list As String
        Dim lists() As String
        Dim oldHost() As Byte
        Dim fileReader As String = ""
        Dim original As String = ""
        Dim startpos As Integer = 0

        If My.Computer.FileSystem.FileExists(hostsPath) Then
            File.SetAttributes(hostsPath, System.IO.FileAttributes.Normal)
            fileReader = My.Computer.FileSystem.ReadAllText(hostsPath)
            If fileReader.Contains("## Cold Turkey Entries ##") Then
                startpos = InStr(1, fileReader, "## Cold Turkey Entries ##")
                If startpos <> 0 And startpos <= 2 Then
                    original = ""
                ElseIf startpos = 0 Then
                    original = fileReader
                Else
                    original = Microsoft.VisualBasic.Left(fileReader, startpos - 2)
                End If
                My.Computer.FileSystem.WriteAllText(hostsPath, original, False)
            End If
        Else
            My.Computer.FileSystem.WriteAllText(hostsPath, "", True)
        End If

        Try
            Dim fs As FileStream
            Dim sw As StreamWriter

            iniFile.Load(configPath)
            list = iniFile.GetKeyValue("main", "websitesBlocked").Replace("""", "")
            lists = Split(encryption.Decrypt3DES(list), ",")

            fs = New FileStream(hostsPath, FileMode.Append, FileAccess.Write, FileShare.Read)
            sw = New StreamWriter(fs)
            sw.WriteLine(vbNewLine & "## Cold Turkey Entries ##" & vbNewLine)
            For Each entry In lists
                If entry.Length <> 0 Then
                    If (entry = "facebook.com") Then
                        sw.WriteLine("0.0.0.0 facebook.com" _
                        + vbNewLine + "0.0.0.0 www.facebook.com" _
                        + vbNewLine + "0.0.0.0 api.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 apps.facebook.com" _
                        + vbNewLine + "0.0.0.0 apps.new.facebook.com" _
                        + vbNewLine + "0.0.0.0 badge.facebook.com" _
                        + vbNewLine + "0.0.0.0 blog.facebook.com" _
                        + vbNewLine + "0.0.0.0 ck.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 da-dk.facebook.com" _
                        + vbNewLine + "0.0.0.0 de-de.facebook.com" _
                        + vbNewLine + "0.0.0.0 de.facebook.com" _
                        + vbNewLine + "0.0.0.0 depauw.facebook.com" _
                        + vbNewLine + "0.0.0.0 developer.facebook.com" _
                        + vbNewLine + "0.0.0.0 developers.facebook.com" _
                        + vbNewLine + "0.0.0.0 el-gr.facebook.com" _
                        + vbNewLine + "0.0.0.0 en-gb.facebook.com" _
                        + vbNewLine + "0.0.0.0 en-us.facebook.com" _
                        + vbNewLine + "0.0.0.0 es-es.facebook.com" _
                        + vbNewLine + "0.0.0.0 es-la.facebook.com" _
                        + vbNewLine + "0.0.0.0 fr-fr.facebook.com" _
                        + vbNewLine + "0.0.0.0 fr.facebook.com" _
                        + vbNewLine + "0.0.0.0 fsu.facebook.com" _
                        + vbNewLine + "0.0.0.0 hs.facebook.com" _
                        + vbNewLine + "0.0.0.0 hy-am.facebook.com" _
                        + vbNewLine + "0.0.0.0 iphone.facebook.com" _
                        + vbNewLine + "0.0.0.0 it-it.facebook.com" _
                        + vbNewLine + "0.0.0.0 ja-jp.facebook.com" _
                        + vbNewLine + "0.0.0.0 lite.facebook.com" _
                        + vbNewLine + "0.0.0.0 login.facebook.com" _
                        + vbNewLine + "0.0.0.0 m.facebook.com" _
                        + vbNewLine + "0.0.0.0 new.facebook.com" _
                        + vbNewLine + "0.0.0.0 nn-no.facebook.com" _
                        + vbNewLine + "0.0.0.0 northland.facebook.com" _
                        + vbNewLine + "0.0.0.0 nyu.facebook.com" _
                        + vbNewLine + "0.0.0.0 presby.facebook.com" _
                        + vbNewLine + "0.0.0.0 profile.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 pt-br.facebook.com" _
                        + vbNewLine + "0.0.0.0 pt-pt.facebook.com" _
                        + vbNewLine + "0.0.0.0 ru-ru.facebook.com" _
                        + vbNewLine + "0.0.0.0 static.ak.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 static.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 static.new.facebook.com" _
                        + vbNewLine + "0.0.0.0 sv-se.facebook.com" _
                        + vbNewLine + "0.0.0.0 te-in.facebook.com" _
                        + vbNewLine + "0.0.0.0 th-th.facebook.com" _
                        + vbNewLine + "0.0.0.0 touch.facebook.com" _
                        + vbNewLine + "0.0.0.0 tr-tr.facebook.com" _
                        + vbNewLine + "0.0.0.0 ufl.facebook.com" _
                        + vbNewLine + "0.0.0.0 uillinois.facebook.com" _
                        + vbNewLine + "0.0.0.0 uppsalauni.facebook.com" _
                        + vbNewLine + "0.0.0.0 video.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 vthumb.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 wiki.developers.facebook.com" _
                        + vbNewLine + "0.0.0.0 wm.facebook.com" _
                        + vbNewLine + "0.0.0.0 ww.facebook.com" _
                        + vbNewLine + "0.0.0.0 zh-cn.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.api.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.apps.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.apps.new.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.badge.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.blog.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.ck.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.da-dk.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.de-de.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.de.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.depauw.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.developer.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.developers.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.el-gr.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.en-gb.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.en-us.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.es-es.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.es-la.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.fr-fr.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.fr.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.fsu.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.hs.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.hy-am.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.iphone.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.it-it.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.ja-jp.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.lite.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.login.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.m.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.new.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.nn-no.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.northland.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.nyu.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.presby.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.profile.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.pt-br.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.pt-pt.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.ru-ru.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.static.ak.connect.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.static.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.static.new.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.sv-se.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.te-in.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.th-th.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.touch.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.tr-tr.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.ufl.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.uillinois.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.uppsalauni.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.video.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.vthumb.ak.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.wiki.developers.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.wm.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.ww.facebook.com" _
                        + vbNewLine + "0.0.0.0 www.zh-cn.facebook.com")
                    ElseIf (entry = "myspace.com") Then
                        sw.WriteLine("0.0.0.0 myspace.com" _
                        + vbNewLine + "0.0.0.0 www.myspace.com" _
                        + vbNewLine + "0.0.0.0 api.myspace.com" _
                        + vbNewLine + "0.0.0.0 ar.myspace.com" _
                        + vbNewLine + "0.0.0.0 at.myspace.com" _
                        + vbNewLine + "0.0.0.0 au.myspace.com" _
                        + vbNewLine + "0.0.0.0 b.myspace.com" _
                        + vbNewLine + "0.0.0.0 belgie.myspace.com" _
                        + vbNewLine + "0.0.0.0 belgique.myspace.com" _
                        + vbNewLine + "0.0.0.0 blog.myspace.com" _
                        + vbNewLine + "0.0.0.0 blogs.myspace.com" _
                        + vbNewLine + "0.0.0.0 br.myspace.com" _
                        + vbNewLine + "0.0.0.0 browseusers.myspace.com" _
                        + vbNewLine + "0.0.0.0 bulletins.myspace.com" _
                        + vbNewLine + "0.0.0.0 ca.myspace.com" _
                        + vbNewLine + "0.0.0.0 cf.myspace.com" _
                        + vbNewLine + "0.0.0.0 collect.myspace.com" _
                        + vbNewLine + "0.0.0.0 comment.myspace.com" _
                        + vbNewLine + "0.0.0.0 cp.myspace.com" _
                        + vbNewLine + "0.0.0.0 creative-origin.myspace.com" _
                        + vbNewLine + "0.0.0.0 de.myspace.com" _
                        + vbNewLine + "0.0.0.0 dk.myspace.com" _
                        + vbNewLine + "0.0.0.0 es.myspace.com" _
                        + vbNewLine + "0.0.0.0 events.myspace.com" _
                        + vbNewLine + "0.0.0.0 fi.myspace.com" _
                        + vbNewLine + "0.0.0.0 forum.myspace.com" _
                        + vbNewLine + "0.0.0.0 forums.myspace.com" _
                        + vbNewLine + "0.0.0.0 fr.myspace.com" _
                        + vbNewLine + "0.0.0.0 friends.myspace.com" _
                        + vbNewLine + "0.0.0.0 groups.myspace.com" _
                        + vbNewLine + "0.0.0.0 home.myspace.com" _
                        + vbNewLine + "0.0.0.0 ie.myspace.com" _
                        + vbNewLine + "0.0.0.0 in.myspace.com" _
                        + vbNewLine + "0.0.0.0 invites.myspace.com" _
                        + vbNewLine + "0.0.0.0 it.myspace.com" _
                        + vbNewLine + "0.0.0.0 jobs.myspace.com" _
                        + vbNewLine + "0.0.0.0 jp.myspace.com" _
                        + vbNewLine + "0.0.0.0 ksolo.myspace.com" _
                        + vbNewLine + "0.0.0.0 la.myspace.com" _
                        + vbNewLine + "0.0.0.0 lads.myspace.com" _
                        + vbNewLine + "0.0.0.0 latino.myspace.com" _
                        + vbNewLine + "0.0.0.0 m.myspace.com" _
                        + vbNewLine + "0.0.0.0 mediaservices.myspace.com" _
                        + vbNewLine + "0.0.0.0 messaging.myspace.com" _
                        + vbNewLine + "0.0.0.0 music.myspace.com" _
                        + vbNewLine + "0.0.0.0 mx.myspace.com" _
                        + vbNewLine + "0.0.0.0 nl.myspace.com" _
                        + vbNewLine + "0.0.0.0 no.myspace.com" _
                        + vbNewLine + "0.0.0.0 nz.myspace.com" _
                        + vbNewLine + "0.0.0.0 pl.myspace.com" _
                        + vbNewLine + "0.0.0.0 profile.myspace.com" _
                        + vbNewLine + "0.0.0.0 profileedit.myspace.com" _
                        + vbNewLine + "0.0.0.0 pt.myspace.com" _
                        + vbNewLine + "0.0.0.0 ru.myspace.com" _
                        + vbNewLine + "0.0.0.0 school.myspace.com" _
                        + vbNewLine + "0.0.0.0 schweiz.myspace.com" _
                        + vbNewLine + "0.0.0.0 se.myspace.com" _
                        + vbNewLine + "0.0.0.0 searchservice.myspace.com" _
                        + vbNewLine + "0.0.0.0 secure.myspace.com" _
                        + vbNewLine + "0.0.0.0 signups.myspace.com" _
                        + vbNewLine + "0.0.0.0 suisse.myspace.com" _
                        + vbNewLine + "0.0.0.0 svizzera.myspace.com" _
                        + vbNewLine + "0.0.0.0 tr.myspace.com" _
                        + vbNewLine + "0.0.0.0 uk.myspace.com" _
                        + vbNewLine + "0.0.0.0 us.myspace.com" _
                        + vbNewLine + "0.0.0.0 vids.myspace.com" _
                        + vbNewLine + "0.0.0.0 viewmorepics.myspace.com" _
                        + vbNewLine + "0.0.0.0 zh.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.api.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.ar.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.at.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.au.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.b.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.belgie.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.belgique.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.blog.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.blogs.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.br.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.browseusers.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.bulletins.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.ca.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.cf.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.collect.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.comment.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.cp.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.creative-origin.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.de.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.dk.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.es.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.events.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.fi.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.forum.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.forums.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.fr.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.friends.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.groups.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.home.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.ie.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.in.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.invites.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.it.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.jobs.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.jp.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.ksolo.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.la.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.lads.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.latino.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.m.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.mediaservices.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.messaging.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.music.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.mx.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.nl.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.no.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.nz.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.pl.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.profile.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.profileedit.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.pt.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.ru.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.school.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.schweiz.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.se.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.searchservice.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.secure.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.signups.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.suisse.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.svizzera.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.tr.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.uk.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.us.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.vids.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.viewmorepics.myspace.com" _
                        + vbNewLine + "0.0.0.0 www.zh.myspace.com")
                    ElseIf (entry = "twitter.com") Then
                        sw.WriteLine("0.0.0.0 twitter.com" _
                        + vbNewLine + "0.0.0.0 www.twitter.com" _
                        + vbNewLine + "0.0.0.0 apiwiki.twitter.com" _
                        + vbNewLine + "0.0.0.0 assets0.twitter.com" _
                        + vbNewLine + "0.0.0.0 assets3.twitter.com" _
                        + vbNewLine + "0.0.0.0 blog.fr.twitter.com" _
                        + vbNewLine + "0.0.0.0 blog.twitter.com" _
                        + vbNewLine + "0.0.0.0 business.twitter.com" _
                        + vbNewLine + "0.0.0.0 chirp.twitter.com" _
                        + vbNewLine + "0.0.0.0 dev.twitter.com" _
                        + vbNewLine + "0.0.0.0 explore.twitter.com" _
                        + vbNewLine + "0.0.0.0 help.twitter.com" _
                        + vbNewLine + "0.0.0.0 integratedsearch.twitter.com" _
                        + vbNewLine + "0.0.0.0 m.twitter.com" _
                        + vbNewLine + "0.0.0.0 mobile.twitter.com" _
                        + vbNewLine + "0.0.0.0 mobile.blog.twitter.com" _
                        + vbNewLine + "0.0.0.0 platform.twitter.com" _
                        + vbNewLine + "0.0.0.0 search.twitter.com" _
                        + vbNewLine + "0.0.0.0 static.twitter.com" _
                        + vbNewLine + "0.0.0.0 status.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.apiwiki.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.assets0.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.assets3.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.blog.fr.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.blog.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.business.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.chirp.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.dev.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.explore.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.help.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.integratedsearch.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.m.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.mobile.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.mobile.blog.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.platform.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.search.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.static.twitter.com" _
                        + vbNewLine + "0.0.0.0 www.status.twitter.com")
                    ElseIf (entry = "youtube.com") Then
                        sw.WriteLine("0.0.0.0 youtube.com" _
                        + vbNewLine + "0.0.0.0 www.youtube.com" _
                        + vbNewLine + "0.0.0.0 apiblog.youtube.com" _
                        + vbNewLine + "0.0.0.0 au.youtube.com" _
                        + vbNewLine + "0.0.0.0 ca.youtube.com" _
                        + vbNewLine + "0.0.0.0 de.youtube.com" _
                        + vbNewLine + "0.0.0.0 es.youtube.com" _
                        + vbNewLine + "0.0.0.0 fr.youtube.com" _
                        + vbNewLine + "0.0.0.0 gdata.youtube.com" _
                        + vbNewLine + "0.0.0.0 help.youtube.com" _
                        + vbNewLine + "0.0.0.0 hk.youtube.com" _
                        + vbNewLine + "0.0.0.0 img.youtube.com" _
                        + vbNewLine + "0.0.0.0 in.youtube.com" _
                        + vbNewLine + "0.0.0.0 it.youtube.com" _
                        + vbNewLine + "0.0.0.0 jp.youtube.com" _
                        + vbNewLine + "0.0.0.0 m.youtube.com" _
                        + vbNewLine + "0.0.0.0 nl.youtube.com" _
                        + vbNewLine + "0.0.0.0 nz.youtube.com" _
                        + vbNewLine + "0.0.0.0 uk.youtube.com" _
                        + vbNewLine + "0.0.0.0 upload.youtube.com" _
                        + vbNewLine + "0.0.0.0 web1.nyc.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.apiblog.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.au.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.ca.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.de.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.es.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.fr.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.gdata.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.help.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.hk.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.img.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.in.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.it.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.jp.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.m.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.nl.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.nz.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.uk.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.upload.youtube.com" _
                        + vbNewLine + "0.0.0.0 www.web1.nyc.youtube.com")
                    ElseIf (entry = "live.com") = True Then
                        sw.WriteLine("0.0.0.0 live.com" _
                        + vbNewLine + "0.0.0.0 www.live.com" _
                        + vbNewLine + "0.0.0.0 favorites.live.com" _
                        + vbNewLine + "0.0.0.0 g.live.com" _
                        + vbNewLine + "0.0.0.0 gallery.live.com" _
                        + vbNewLine + "0.0.0.0 get.live.com" _
                        + vbNewLine + "0.0.0.0 groups.live.com" _
                        + vbNewLine + "0.0.0.0 home.live.com" _
                        + vbNewLine + "0.0.0.0 home.spaces.live.com" _
                        + vbNewLine + "0.0.0.0 hotmail.live.com" _
                        + vbNewLine + "0.0.0.0 ideas.live.com" _
                        + vbNewLine + "0.0.0.0 im.live.com" _
                        + vbNewLine + "0.0.0.0 images.domains.live.com" _
                        + vbNewLine + "0.0.0.0 intl.local.live.com" _
                        + vbNewLine + "0.0.0.0 local.live.com" _
                        + vbNewLine + "0.0.0.0 localsearch.live.com" _
                        + vbNewLine + "0.0.0.0 login.live.com" _
                        + vbNewLine + "0.0.0.0 lsrvsc.spaces.live.com" _
                        + vbNewLine + "0.0.0.0 mail.live.com" _
                        + vbNewLine + "0.0.0.0 messenger.live.com" _
                        + vbNewLine + "0.0.0.0 messenger.services.live.com" _
                        + vbNewLine + "0.0.0.0 mobile.live.com" _
                        + vbNewLine + "0.0.0.0 my.live.com" _
                        + vbNewLine + "0.0.0.0 people.live.com" _
                        + vbNewLine + "0.0.0.0 photo.live.com" _
                        + vbNewLine + "0.0.0.0 profile.live.com" _
                        + vbNewLine + "0.0.0.0 qna.live.com" _
                        + vbNewLine + "0.0.0.0 settings.messenger.live.com" _
                        + vbNewLine + "0.0.0.0 shared.live.com" _
                        + vbNewLine + "0.0.0.0 spaces.live.com" _
                        + vbNewLine + "0.0.0.0 tou.live.com" _
                        + vbNewLine + "0.0.0.0 www.favorites.live.com" _
                        + vbNewLine + "0.0.0.0 www.g.live.com" _
                        + vbNewLine + "0.0.0.0 www.gallery.live.com" _
                        + vbNewLine + "0.0.0.0 www.get.live.com" _
                        + vbNewLine + "0.0.0.0 www.groups.live.com" _
                        + vbNewLine + "0.0.0.0 www.home.live.com" _
                        + vbNewLine + "0.0.0.0 www.home.spaces.live.com" _
                        + vbNewLine + "0.0.0.0 www.hotmail.live.com" _
                        + vbNewLine + "0.0.0.0 www.ideas.live.com" _
                        + vbNewLine + "0.0.0.0 www.im.live.com" _
                        + vbNewLine + "0.0.0.0 www.images.domains.live.com" _
                        + vbNewLine + "0.0.0.0 www.intl.local.live.com" _
                        + vbNewLine + "0.0.0.0 www.local.live.com" _
                        + vbNewLine + "0.0.0.0 www.localsearch.live.com" _
                        + vbNewLine + "0.0.0.0 www.login.live.com" _
                        + vbNewLine + "0.0.0.0 www.lsrvsc.spaces.live.com" _
                        + vbNewLine + "0.0.0.0 www.mail.live.com" _
                        + vbNewLine + "0.0.0.0 www.messenger.live.com" _
                        + vbNewLine + "0.0.0.0 www.messenger.services.live.com" _
                        + vbNewLine + "0.0.0.0 www.mobile.live.com" _
                        + vbNewLine + "0.0.0.0 www.my.live.com" _
                        + vbNewLine + "0.0.0.0 www.people.live.com" _
                        + vbNewLine + "0.0.0.0 www.photo.live.com" _
                        + vbNewLine + "0.0.0.0 www.profile.live.com" _
                        + vbNewLine + "0.0.0.0 www.qna.live.com" _
                        + vbNewLine + "0.0.0.0 www.settings.messenger.live.com" _
                        + vbNewLine + "0.0.0.0 www.shared.live.com" _
                        + vbNewLine + "0.0.0.0 www.spaces.live.com" _
                        + vbNewLine + "0.0.0.0 www.tou.live.com")
                    Else
                        sw.WriteLine("0.0.0.0 " & entry)
                        sw.WriteLine("0.0.0.0 www." & entry)
                    End If
                End If
            Next
            sw.Close()
            fs.Close()

            oldHost = ASCIIEncoding.ASCII.GetBytes(File.ReadAllText(hostsPath))
            oldHashHosts = New MD5CryptoServiceProvider().ComputeHash(oldHost)

        Catch ee As Exception
            My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\error.log", ee.ToString, True)
            stopBlock()
            End
        End Try

    End Sub

    Private Sub stopBlock()

        Dim iniFile As New IniFile
        Dim fileReader As String = ""
        Dim original As String = ""
        Dim startpos As Integer = 0

        If My.Computer.FileSystem.FileExists(hostsPath) Then
            File.SetAttributes(hostsPath, System.IO.FileAttributes.Normal)
            fileReader = My.Computer.FileSystem.ReadAllText(hostsPath)
            If fileReader.Contains("## Cold Turkey Entries ##") Then
                startpos = InStr(1, fileReader, "## Cold Turkey Entries ##")
                If startpos <> 0 And startpos <= 2 Then
                    original = ""
                ElseIf startpos = 0 Then
                    original = fileReader
                Else
                    original = Microsoft.VisualBasic.Left(fileReader, startpos - 2)
                End If
                My.Computer.FileSystem.WriteAllText(hostsPath, original, False)
            End If
            File.SetAttributes(hostsPath, System.IO.FileAttributes.ReadOnly)
        End If
    End Sub

    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String
        Dim i As Integer
        Dim sOutput As New StringBuilder(arrInput.Length)
        For i = 0 To arrInput.Length - 1
            sOutput.Append(arrInput(i).ToString("X2"))
        Next
        Return sOutput.ToString()
    End Function
End Class
Public Class Crypto3DES

    Public Sub New()
    End Sub

    Private m_encoding As System.Text.Encoding


    Public ReadOnly Property Key() As String
        Get
            Return "CTServic"
        End Get
    End Property


    Public Property Encoding() As System.Text.Encoding
        Get
            If m_encoding Is Nothing Then
                m_encoding = System.Text.Encoding.UTF8
            End If
            Return m_encoding
        End Get

        Set(value As System.Text.Encoding)
            m_encoding = value
        End Set
    End Property


    Public Function Encrypt3DES(strString As String) As String
        Dim DES As New DESCryptoServiceProvider()

        DES.Key = Encoding.GetBytes(Me.Key)
        DES.Mode = CipherMode.ECB
        DES.Padding = PaddingMode.Zeros

        Dim DESEncrypt As ICryptoTransform = DES.CreateEncryptor()

        Dim Buffer As Byte() = m_encoding.GetBytes(strString)

        Return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length))
    End Function


    Public Function Decrypt3DES(strString As String) As String
        Dim DES As New DESCryptoServiceProvider()

        DES.Key = Encoding.UTF8.GetBytes(Me.Key)
        DES.Mode = CipherMode.ECB
        DES.Padding = PaddingMode.Zeros
        Dim DESDecrypt As ICryptoTransform = DES.CreateDecryptor()

        Dim Buffer As Byte() = Convert.FromBase64String(strString)
        Return UTF8Encoding.UTF8.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length))
    End Function
End Class
