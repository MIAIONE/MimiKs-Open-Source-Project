Imports System.Drawing
Imports System.Console
Imports System.GC
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Thread

Public Module ApplicationStartUp

    Public Function ImgZoon(img As Image, x As Integer) As Image '缩放图片
        If x > 0 Then
            img = New Bitmap(img, img.Width * x, img.Height * x)
        ElseIf x < 0 Then
            x = Math.Abs(x)
            img = New Bitmap(img, img.Width / x, img.Height / x)
        ElseIf x = 1 Then
            Return img
        ElseIf x = -1 Then
            Return img
        End If
        Return img
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Public Function GetConsoleWindow() As IntPtr '获得控制台句柄
    End Function
    Public ImageCount As Integer = 5262
    Public H As IntPtr = GetConsoleWindow()
    Public FPS As Integer = 0
    Public ImageEX As Image
    Public COUNT As Integer = 0
    Public ReadOnly mBrush As New SolidBrush(Color.RoyalBlue)
    Public ReadOnly mXBrush As New SolidBrush(Color.White)
    Public ReadOnly mFont As New Font("微软雅黑", 11, FontStyle.Bold)
    Public ReadOnly mXFont As New Font("微软雅黑", 16, FontStyle.Italic)

    Public ReadOnly mPen As New Pen(mXBrush, 42)
    Public ReadOnly mXPen As New Pen(mXBrush, 64000000)

    Public ReadOnly TicketSub As New Thread(AddressOf Ticket)
    Public ReadOnly Info As String = vbCrLf + Application.ProductName + " V" + Application.ProductVersion + vbCrLf + vbCrLf + "Window Size : " + WindowWidth.ToString + "x" + WindowHeight.ToString + vbCrLf + vbCrLf + "按回车键(Enter)开始播放"

    Public ReadOnly IMPATH As String = Application.StartupPath.Replace("EXEOut", "") '这个是自动获取路径的，改成其他的图片注意这里
    'Replace作用->替换路径
    '    源路径 ：D:\XXX\Image
    '    程序路径：D:\XXX\APP\XXX.EXE
    '   Replace("D:\XXX\APP\XXX.EXE","APP\XXX.EXE","Image")
    '   这就通过D:\XXX\APP\XXX.EXE获取到了D:\XXX\Image的相对路径
    '
    Public ReadOnly States As New Thread(AddressOf STAThread)
    Public ImageNew As Image
    Public ImageSize As String = ""
    Public OK_KNOW As Boolean = False
    Public GDISub As Thread
    <STAThread>'STAThread支持线程隔离
    Public Sub STAThread()
        While True '设置图片总数，必要时跳出循环，防止报错
            Recycle() '回收垃圾，防止内存爆炸，虽然对帧率有影响，但是不回收又会爆内存和出错
            Try
                GDISub = Nothing
                GDISub = New Thread(AddressOf GDI_Threading)
                FPS += 1
                COUNT += 1
                If COUNT = ImageCount Then Exit While
                ImageEX = Nothing
                ImageEX = Image.FromFile(IMPATH + "\Image\" + "(" + COUNT.ToString + ").jpg") '文件名格式  "(1).jpg"、"(2).jpg".。。。。
                ImageNew = ImageEX 'ImgZoon(ImageEX, 1) '缩放功能，缩放会掉帧，但是加了之后对于30fps 1080P的视频正好帧率在30fps
                If OK_KNOW = False Then
                    ImageSize = “  ImageSize: ” + ImageNew.Width.ToString + "x" + ImageNew.Height.ToString
                    OK_KNOW = True
                End If
                GDISub.Start()
            Catch ex As Exception
                GDISub = Nothing '及时销毁GDI线程，防止重复声明错误
            End Try

        End While
        Try
            Dim STARTP As New Point(0, 0)
            Dim ENDP As New Point(1000, 0)
            While True
                Using G As Graphics = Graphics.FromHwnd(H)
                    G.DrawLine(mXPen, STARTP, ENDP)
                    G.DrawString(vbCrLf + vbCrLf + "按回车键(Enter)退出", mXFont, mBrush, 80, 0)
                End Using
                Dim InputKey As ConsoleKey
                If KeyAvailable Then
                    InputKey = ReadKey(True).Key
                    If InputKey = ConsoleKey.Enter Then Exit While
                End If
            End While
        Catch ex As Exception
            End
        End Try
    End Sub

    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.White
        Title = "Administrator | " + "OpenGDI V3" + " | " + Environment.OSVersion.VersionString + " | " + Application.ExecutablePath
        Application.SafeTopLevelCaptionFormat = Title
        Dim STARTP As New Point(0, 0)
        Dim ENDP As New Point(1000, 0)
        While True
            Using G As Graphics = Graphics.FromHwnd(H)
                G.DrawLine(mXPen, STARTP, ENDP)
                G.DrawString(Info, mXFont, mBrush, 80, 0)
            End Using
            Dim InputKey As ConsoleKey
            If KeyAvailable Then
                InputKey = ReadKey(True).Key
                If InputKey = ConsoleKey.Enter Then Exit While
            End If
        End While
        'Sleep(3000)

        TicketSub.Start()
        Clear()
        '高速的循环可以使得图像在窗口变化下仍然存在

        States.Start()

        Application.VisualStyleState = VisualStyles.VisualStyleState.ClientAndNonClientAreasEnabled
        Application.Run()
        'Clear()
        'WriteLine("播放完毕")
        'ReadKey()
    End Sub

    <STAThread>
    Public Sub GDI_Threading()
ReTry:
        Try
            '核心：GDI画图
            Using G As Graphics = Graphics.FromHwnd(H)
                G.DrawImage(ImageNew, New Rectangle(0, (CursorTop + 1) * CursorSize - 5, ImageNew.Width, ImageNew.Height), New Rectangle(0, 0, ImageNew.Width, ImageNew.Height), GraphicsUnit.Pixel)
                ' If FPS >= 30 Then Sleep(20)  '这个根据性能测试下，我的电脑设置成10毫秒刚刚30fps
                ' If FPS >= 40 Then Sleep(80)
                ' If FPS >= 50 Then Sleep(100)
                ' If FPS >= 60 Then Sleep(160)
            End Using
        Catch ex As Exception
            GoTo ReTry
        End Try
    End Sub


    <STAThread>
    Public Sub Ticket() '显示FPS的子线程
        On Error Resume Next
        ForegroundColor = ConsoleColor.White
        BackgroundColor = ConsoleColor.White
        Dim STARTP As New Point(0, 0)
        Dim ENDP As New Point(1000, 0)
        While True
            Sleep(1000)
            Using G As Graphics = Graphics.FromHwnd(H)
                G.DrawLine(mPen, STARTP, ENDP)
                G.DrawString("FPS:" + FPS.ToString + "  OpenGDI V3  Window Size : " + WindowWidth.ToString + "x" + WindowHeight.ToString + ImageSize, mFont, mBrush, 20, 0)
            End Using
            FPS = 0
        End While
    End Sub
    <STAThread>
    Public Sub Recycle()
        '你不加GC内存占用会爆炸的
        Collect()
    End Sub

End Module