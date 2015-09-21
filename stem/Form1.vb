Imports System
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Data
Imports System.Data.OleDb

Public Class Form1
    Dim str(500), strtemp, stemmedstr(500) As String
    Dim sep(10) As Char
    Dim con As System.Data.SqlClient.SqlConnection
    Dim cmd As System.Data.SqlClient.SqlCommand
    Dim dr As System.Data.SqlClient.SqlDataReader
    Dim objDS As New Data.DataSet()
    Dim objDA As SqlClient.SqlDataAdapter
    Dim sqlStr As String

    Public b As Char()
    Private i As Integer                  ' offset into b
    Private i_end As Integer                  ' offset to end of stemmed word
    Private j, k As Integer
    Private Shared INC As Integer = 200                  ' unit of size whereby b is increased
    'Public Sub New()
    '    b = New Char(INC) {}
    '    i = 0
    '    i_end = 0
    'End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim widt As Integer
        widt = Convert.ToInt32(ListView1.Width) / 2
        ListView1.Columns.Add("Stemmed Words", widt, HorizontalAlignment.Center)
        '    ListView1.Columns.Add("Irrelevant Words", widt, HorizontalAlignment.Center)
        ListView1.Columns.Add("Term Frequency (TF)", widt, HorizontalAlignment.Center)
        sep(0) = " "
    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim punct() As String = {"?", ",", ".", "<", ">", ";", ":", "{", "}", "[", "]", "!", "&"}
        Dim stri, keyw(500), li(2), irr(500), st, sto(500) As String
        Dim oread As StreamReader
        Dim a, b As New List(Of String)
        Dim ix, curr, ip As Integer
        Dim itm As ListViewItem
        curr = ix = 0
        ListView1.Items.Clear()
        If TextBox1.Text <> "" And TextBox2.Text <> "" Then
            oread = File.OpenText(Application.StartupPath + "\stop words.txt")
            While oread.Peek <> -1
                b.Add(oread.ReadLine.ToLower)
            End While
            oread.Close()
            stri = TextBox1.Text.Trim.ToLower + " " + TextBox2.Text.Trim.ToLower
            For index = 0 To punct.Length - 1 Step 1
                stri = stri.Replace(punct(index), "")
            Next

            strtemp = stri
            str = strtemp.Trim.Split(sep)
            ' TextBox3.Text = stemTerm(strtemp)

            For index = 0 To str.Length - 1 Step 1
                If b.Contains(str(index)) Then
                    irr(ix) = str(index)
                    ix = ix + 1
                Else
                    stemmedstr(ip) = stemTerm(str(index))
                    ip = ip + 1
                End If
            Next


            For indexx = 0 To ix - 1
                ListBox1.Items.Add(irr(indexx))
            Next
            ix = 0
            Do While ix < ip
                If Not a.Contains(stemmedstr(ix)) Then
                    a.Add(stemmedstr(ix))
                End If
                ix = ix + 1
            Loop
            keyw = a.ToArray
            Dim count(keyw.Length) As Integer
            For indexx = 0 To keyw.Length - 1
                For indexy = 0 To ip - 1
                    If keyw(indexx).Equals(stemmedstr(indexy)) Then
                        count(indexx) = count(indexx) + 1
                    End If
                Next
            Next
            For indexx = 0 To keyw.Length - 1 Step 1
                li(0) = keyw(indexx)
                TextBox3.Text = LTrim(TextBox3.Text + " " + keyw(indexx))
                li(1) = count(indexx)
                itm = New ListViewItem(li)
                ListView1.Items.Add(itm)
            Next
        Else
            MsgBox("You have missed the problem type (or) Subject (or) Problem Definition", MsgBoxStyle.OkOnly, "Missed something")
        End If
        
    End Sub

    Public Function stemTerm(ByVal s As String) As String
        setTerm(s)
        stem()
        Return getTerm()
    End Function

    '        SetTerm and GetTerm have been simply added to ease the
    '        interface with other lanaguages. They replace the add functions
    '        and toString function. This was done because the original functions stored
    '        all stemmed words (and each time a new woprd was added, the buffer would be
    '        re-copied each time, making it quite slow). Now, The class interface
    '        that is provided simply accepts a term and returns its stem,
    '        instead of storing all stemmed words.

    Private Sub setTerm(ByVal s As String)
        i = s.Length
        Dim new_b As Char() = New Char(i) {}
        Dim c As Integer
        For c = 0 To (i - 1)
            new_b(c) = s.Chars(c)
        Next
        b = new_b
    End Sub

    '  Stem the word placed into the Stemmer buffer through calls to add().
    '  Returns true if the stemming process resulted in a word different
    '  from the input.  You can retrieve the result with
    '  getResultLength()/getResultBuffer() or toString().
    '
    Public Sub stem()
        k = i - 1
        If (k > 1) Then
            step1()
            step2()
            step3()
            step4()
            step5()
            step6()
        End If
        i_end = k + 1
        i = 0
    End Sub


    '  step1() gets rid of plurals and -ed or -ing. e.g.
    '           caresses  ->  caress
    '           ponies    ->  poni
    '           ties      ->  ti
    '           caress    ->  caress
    '           cats      ->  cat
    '
    '           feed      ->  feed
    '           agreed    ->  agree
    '           disabled  ->  disable
    '
    '           matting   ->  mat
    '           mating    ->  mate
    '           meeting   ->  meet
    '           milling   ->  mill
    '           messing   ->  mess
    '
    '           meetings  ->  meet
    '
    Private Sub step1()
        If (b(k) = "s"c) Then
            If (ends("sses")) Then
                k = k - 2
            ElseIf (ends("ies")) Then
                setto("i")
            ElseIf (b(k - 1) <> "s"c) Then
                k = k - 1
            End If
        End If
        If (ends("eed")) Then
            If (m() > 0) Then
                k = k - 1
            End If
        ElseIf ((ends("ed") OrElse ends("ing")) AndAlso vowelinstem()) Then
            k = j
            If (ends("at")) Then
                setto("ate")
            ElseIf (ends("bl")) Then
                setto("ble")
            ElseIf (ends("iz")) Then
                setto("ize")
            ElseIf (doublec(k)) Then
                k = k - 1
                Dim ch As Char = b(k)
                If ((ch = "l"c) OrElse (ch = "s"c) OrElse (ch = "z"c)) Then
                    k = k + 1
                End If
            ElseIf ((m() = 1) AndAlso cvc(k)) Then
                setto("e")
            End If
        End If
    End Sub

    '  step2() turns terminal y to i when there is another vowel in the stem.
    Private Sub step2()
        If (ends("y") AndAlso vowelinstem()) Then
            b(k) = "i"c
        End If

    End Sub

    '  step3() maps double suffices to single ones. so -ization ( = -ize plus
    '  -ation) maps to -ize etc. note that the string before the suffix must give
    '  m() > 0.
    Private Sub step3()
        If (k = 0) Then Return

        'For Bug 1
        Select Case (b(k - 1))
            Case "a"c
                If ends("ational") Then
                    r("ate")
                    Exit Select
                End If
                If ends("tional") Then
                    r("tion")
                    Exit Select
                End If
                Exit Select

            Case "c"c
                If ends("enci") Then
                    r("ence")
                    Exit Select
                End If
                If ends("anci") Then
                    r("ance")
                    Exit Select
                End If
                Exit Select

            Case "e"c
                If ends("izer") Then
                    r("ize")
                    Exit Select
                End If
                Exit Select

            Case "l"c
                If ends("bli") Then
                    r("ble")
                    Exit Select
                End If
                If ends("alli") Then
                    r("al")
                    Exit Select
                End If
                If ends("entli") Then
                    r("ent")
                    Exit Select
                End If
                If ends("eli") Then
                    r("e")
                    Exit Select
                End If
                If ends("ousli") Then
                    r("ous")
                    Exit Select
                End If
                Exit Select

            Case "o"c
                If ends("ization") Then
                    r("ize")
                    Exit Select
                End If
                If ends("ation") Then
                    r("ate")
                    Exit Select
                End If
                If ends("ator") Then
                    r("ate")
                    Exit Select
                End If
                Exit Select

            Case "s"c
                If ends("alism") Then
                    r("al")
                    Exit Select
                End If
                If ends("iveness") Then
                    r("ive")
                    Exit Select
                End If
                If ends("fulness") Then
                    r("ful")
                    Exit Select
                End If
                If ends("ousness") Then
                    r("ous")
                    Exit Select
                End If
                Exit Select

            Case "t"c
                If ends("aliti") Then
                    r("al")
                    Exit Select
                End If
                If ends("iviti") Then
                    r("ive")
                    Exit Select
                End If
                If ends("biliti") Then
                    r("ble")
                    Exit Select
                End If
                Exit Select

            Case "g"c
                If ends("logi") Then
                    r("log")
                    Exit Select
                End If
                Exit Select

            Case Else
                Exit Select
        End Select
    End Sub

    '  step4() deals with -ic-, -full, -ness etc. similar strategy to step3.
    Private Sub step4()
        Select Case (b(k))
            Case "e"c
                If ends("icate") Then
                    r("ic")
                    Exit Select
                End If
                If ends("ative") Then
                    r("")
                    Exit Select
                End If
                If ends("alize") Then
                    r("al")
                    Exit Select
                End If
                Exit Select

            Case "i"c
                If ends("iciti") Then
                    r("ic")
                    Exit Select
                End If
                Exit Select

            Case "l"c
                If ends("ical") Then
                    r("ic")
                    Exit Select
                End If
                If ends("ful") Then
                    r("")
                    Exit Select
                End If
                Exit Select

            Case "s"c
                If ends("ness") Then
                    r("")
                    Exit Select
                End If
                Exit Select
        End Select
    End Sub

    '  step5() takes off -ant, -ence etc., in context <c>vcvc<v>.
    Private Sub step5()
        If (k = 0) Then Return

        '  for Bug 1
        Select Case (b(k - 1))
            Case "a"c
                If ends("al") Then
                    Exit Select
                End If
                Return

            Case "c"c
                If ends("ance") Then
                    Exit Select
                End If
                If ends("ence") Then
                    Exit Select
                End If
                Return

            Case "e"c
                If ends("er") Then
                    Exit Select
                End If
                Return

            Case "i"c
                If ends("ic") Then
                    Exit Select
                End If
                Return

            Case "l"c
                If ends("able") Then
                    Exit Select
                End If
                If ends("ible") Then
                    Exit Select
                End If
                Return

            Case "n"c
                If ends("ant") Then
                    Exit Select
                End If
                If ends("ement") Then
                    Exit Select
                End If
                If ends("ment") Then
                    Exit Select
                End If
                '  element etc. not stripped before the m
                If ends("ent") Then
                    Exit Select
                End If
                Return

            Case "o"c
                If ends("ion") AndAlso (j >= 0) AndAlso (b(j) = "s"c OrElse b(j) = "t"c) Then
                    '  j >= 0 fixes Bug 2
                    Exit Select
                End If
                If ends("ou") Then
                    Exit Select
                End If
                Return
                'takes care of -ous

            Case "s"c
                If ends("ism") Then
                    Exit Select
                End If
                Return

            Case "t"c
                If ends("ate") Then
                    Exit Select
                End If
                If ends("iti") Then
                    Exit Select
                End If
                Return

            Case "u"c
                If ends("ous") Then
                    Exit Select
                End If
                Return

            Case "v"c
                If ends("ive") Then
                    Exit Select
                End If
                Return

            Case "z"c
                If ends("ize") Then
                    Exit Select
                End If
                Return

            Case Else
                Return
        End Select
        If (m() > 1) Then k = j
    End Sub

    '  step6() removes a final -e if m() > 1.
    Private Sub step6()
        j = k

        If (b(k) = "e"c) Then
            Dim a As Integer = m()
            If (a > 1) OrElse ((a = 1) AndAlso (Not cvc(k - 1))) Then k = k - 1
        End If
        If (b(k) = "l"c) AndAlso doublec(k) AndAlso (m() > 1) Then k = k - 1

    End Sub

    Private Function getTerm() As String
        Return New String(b, 0, i_end)
    End Function

    '
    ' Add a character to the word being stemmed.  When you are finished
    ' adding characters, you can call stem(void) to stem the word.
    '
    Public Sub add(ByVal ch As Char)
        Dim c As Integer
        If (i = b.Length) Then
            Dim new_b As Char() = New Char(i + INC) {}
            For c = 0 To (i - 1) Step 1
                new_b(c) = b(c)
            Next
            b = new_b
        End If
        b(i) = ch
        i = i + 1
    End Sub

    '  Adds wLen characters to the word being stemmed contained in a portion
    '  of a char[] array. This is like repeated calls of add(char ch), but
    '  faster.
    Public Sub add(ByVal w As Char(), ByVal wLen As Integer)
        Dim c As Integer
        If i + wLen >= b.Length Then
            Dim new_b As Char() = New Char(i + wLen + INC) {}
            For c = 0 To (i - 1) Step 1
                new_b(c) = b(c)
            Next
            b = new_b
        End If
        For c = 0 To (wLen - 1) Step 1
            b(i) = w(c)
            i = i + 1
        Next
    End Sub

    '  After a word has been stemmed, it can be retrieved by toString(),
    '  or a reference to the internal buffer can be retrieved by getResultBuffer
    '  and getResultLength (which is generally more efficient.)
    Public Overrides Function ToString() As String
        Return New String(b, 0, i_end)
    End Function

    '  Returns the length of the word resulting from the stemming process.
    Public Function getResultLength() As Integer
        Return i_end
    End Function

    '  Returns a reference to a character buffer containing the results of
    '  the stemming process.  You also need to consult getResultLength()
    '  to determine the length of the result.
    Public Function getResultBuffer() As Char()
        Return b
    End Function

    '  cons(i) is true <=> b[i] is a consonant.
    Public Function cons(ByVal i As Integer) As Boolean
        Select Case b(i)
            Case "a"c                                  ' Cast string to char. Option Strict On.
            Case "e"c
            Case "i"c
            Case "o"c
            Case "u"c
                Return False
            Case "y"c
                If i = 0 Then
                    Return True
                Else
                    Return Not (cons(i - 1))
                End If
            Case Else
                Return True
        End Select
    End Function

    '  m() measures the number of consonant sequences between 0 and j. if c is
    '  a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
    '  presence,
    '          <c><v>       gives 0
    '          <c>vc<v>     gives 1
    '          <c>vcvc<v>   gives 2
    '          <c>vcvcvc<v> gives 3
    '          ....
    '
    Private Function m() As Integer
        Dim n As Integer = 0
        Dim i As Integer = 0

        While True
            If (i > j) Then Return n
            If (Not cons(i)) Then Exit While
            i = i + 1
        End While
        i = i + 1
        While (True)
            While (True)
                If (i > j) Then Return n
                If (cons(i)) Then Exit While
                i = i + 1
            End While
            i = i + 1
            n = n + 1
            While (True)
                If (i > j) Then Return n
                If (Not cons(i)) Then Exit While
                i = i + 1
            End While
            i = i + 1
        End While
    End Function

    '  vowelinstem() is true <=> 0,...j contains a vowel
    Private Function vowelinstem() As Boolean
        Dim i As Integer
        For i = 0 To j Step 1                         '  i <= j
            If (Not cons(i)) Then Return True
        Next
        Return False
    End Function

    '  doublec(j) is true <=> j,(j-1) contain a double consonant.
    Private Function doublec(ByVal j As Integer) As Boolean
        If (j < 1) Then Return False
        If (b(j) <> b(j - 1)) Then Return False
        Return cons(j)
    End Function

    '  cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
    '  and also if the second c is not w,x or y. this is used when trying to
    '  restore an e at the end of a short word. e.g.
    '
    '          cav(e), lov(e), hop(e), crim(e), but
    '          snow, box, tray.
    '
    Private Function cvc(ByVal i As Integer) As Boolean
        If ((i < 2) OrElse (Not cons(i)) OrElse cons(i - 1) OrElse (Not cons(i - 2))) Then
            Return False
        End If
        Dim ch As Char = b(i)
        If (ch = "w"c OrElse ch = "x"c OrElse ch = "y"c) Then Return False
        Return True
    End Function

    Private Function ends(ByVal s As String) As Boolean
        Dim l As Integer = s.Length
        Dim o As Integer = k - l + 1

        If (o < 0) Then Return False

        Dim sc As Char() = s.ToCharArray
        Dim i As Integer

        For i = 0 To (l - 1) Step 1
            If (b(o + i) <> sc(i)) Then Return False
        Next
        j = k - l

        Return True
    End Function

    '  setto(s) sets (j+1),...k to the characters in the string s, readjusting
    '  k.
    Private Sub setto(ByVal s As String)
        Dim l As Integer = s.Length
        Dim o As Integer = j + 1

        Dim sc As Char() = s.ToCharArray
        For i = 0 To (l - 1) Step 1
            b(o + i) = sc(i)
        Next
        k = j + l
    End Sub

    '  r(s) is used further down.
    Private Sub r(ByVal s As String)
        If (m() > 0) Then setto(s)
    End Sub


   

    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    Label1.Text = stemTerm(TextBox1.Text.Trim)
    'End Sub

    
    Private Sub NewToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NewToolStripMenuItem1.Click
        ComboBox1.SelectedIndex = -1
        TextBox1.Text = ""
        TextBox2.Text = ""
    End Sub

    Private Sub ExitToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem1.Click
        End
    End Sub

    Private Sub CutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CutToolStripMenuItem.Click
        If TextBox1.Focused = True Then
            TextBox1.Cut()
        ElseIf TextBox2.Focused = True Then
            TextBox2.Cut()
        End If
    End Sub

    Private Sub CopyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToolStripMenuItem.Click
        If TextBox1.Focused = True Then
            TextBox1.Copy()
        ElseIf TextBox2.Focused = True Then
            TextBox2.Copy()
        End If
    End Sub

    Private Sub PasteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripMenuItem.Click
        If TextBox1.Focused = True Then
            TextBox1.Paste()
        ElseIf TextBox2.Focused = True Then
            TextBox2.Paste()
        End If
    End Sub

    Private Sub SelectAllToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SelectAllToolStripMenuItem.Click
        If TextBox1.Focused = True Then
            TextBox1.SelectAll()
        ElseIf TextBox2.Focused = True Then
            TextBox2.SelectAll()
        End If
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        MsgBox("This is a part of the Natural Language Processing (NLP) project programming by Vivekanandan Chandramouleeswaran. For contact infenzer@yahoo.in")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim punct() As String = {"?", ",", ".", "<", ">", ";", ":", "{", "}", "[", "]", "!", "&"}
        Dim stri, keyw(500), li(2), irr(500), sto(500) As String
        Dim oread As StreamReader
        Dim a, b As New List(Of String)
        Dim ix, curr, ip As Integer
        'Dim itm As ListViewItem
        Dim strStemmed As String
        curr = ix = 0
        con = New SqlClient.SqlConnection("Data Source=.;Initial Catalog=wofra;User ID=sa;Password=1;Integrated Security=False")
        con.Open()
        objDA = New SqlClient.SqlDataAdapter("select * from paper where StemmedAbstract is null", con)
       
        objDA.Fill(objDS, "paper")


        With objDS.Tables(0)
            b = New List(Of String)
            'Loop through the records and print the values
            For intCounter = 0 To .Rows.Count - 1
                Console.WriteLine(.Rows(intCounter).Item("id"))



                If .Rows(intCounter).Item("AbstractWithoutQuotes") <> "" Then
                    oread = File.OpenText(Application.StartupPath + "\stop words.txt")
                    While oread.Peek <> -1
                        b.Add(oread.ReadLine.ToLower)
                    End While
                    oread.Close()
                    stri = ""
                    stri = .Rows(intCounter).Item("AbstractWithoutQuotes").Trim.ToLower
                    For index = 0 To punct.Length - 1 Step 1
                        stri = stri.Replace(punct(index), "")
                    Next
                    strtemp = ""
                    strtemp = stri
                    str = strtemp.Trim.Split(sep)
                    ' TextBox3.Text = stemTerm(strtemp)
                    ip = 0
                    irr = Nothing
                    ReDim irr(500)
                    For index = 0 To str.Length - 1 Step 1
                        If b.Contains(str(index)) Then
                            irr(ix) = str(index)
                            ix = ix + 1
                        Else
                            stemmedstr(ip) = stemTerm(str(index))
                            ip = ip + 1
                        End If
                    Next


                    'For indexx = 0 To ix - 1
                    '    ListBox1.Items.Add(IRR(indexx))
                    'Next
                    ix = 0
                    a = New List(Of String)
                    Do While ix < ip
                        'If Not a.Contains(stemmedstr(ix)) Then
                        a.Add(stemmedstr(ix))
                        'End If
                        ix = ix + 1
                    Loop
                    'keyw(500) = Nothing
                    keyw = Nothing
                    ReDim keyw(500)
                    'Array.Clear(keyw, 0, keyw.Length)
                    keyw = a.ToArray
                    Dim count(keyw.Length) As Integer
                    For indexx = 0 To keyw.Length - 1
                        For indexy = 0 To ip - 1
                            If keyw(indexx).Equals(stemmedstr(indexy)) Then
                                count(indexx) = count(indexx) + 1
                            End If
                        Next
                    Next
                    strStemmed = ""
                    li = Nothing
                    ReDim li(2)
                    'Array.Clear(li, 0, li.Length)
                    For indexx = 0 To keyw.Length - 1 Step 1
                        li(0) = keyw(indexx)
                        strStemmed = strStemmed + " " + keyw(indexx)
                        li(1) = count(indexx)
                        'itm = New ListViewItem(li)
                        'ListView1.Items.Add(itm)
                    Next
                    'Else
                    'MsgBox("You have missed the problem type (or) Subject (or) Problem Definition", MsgBoxStyle.OkOnly, "Missed something")
                End If


                sqlStr = "update paper set stemmedAbstract='" & strStemmed & "' where id=" & .Rows(intCounter).Item("id")
                cmd = New SqlClient.SqlCommand(sqlStr, con)
                cmd.ExecuteNonQuery()
            Next
            Console.ReadLine()

        End With
        con.Close()
        MessageBox.Show("ready")
    End Sub

    Private Sub btnTitle_Click(sender As Object, e As EventArgs) Handles btnTitle.Click
        Dim punct() As String = {"?", ",", ".", "<", ">", ";", ":", "{", "}", "[", "]", "!", "&"}
        Dim stri, keyw(500), li(2), irr(500), sto(500) As String
        Dim oread As StreamReader
        Dim a, b As New List(Of String)
        Dim ix, curr, ip As Integer
        'Dim itm As ListViewItem
        Dim strStemmed As String
        curr = ix = 0
        con = New SqlClient.SqlConnection("Data Source=.;Initial Catalog=wofra;User ID=sa;Password=1;Integrated Security=False")
        con.Open()
        objDA = New SqlClient.SqlDataAdapter("select * from paper where StemmedTitle is null", con)

        objDA.Fill(objDS, "paper")


        With objDS.Tables(0)
            b = New List(Of String)
            'Loop through the records and print the values
            For intCounter = 0 To .Rows.Count - 1
                Console.WriteLine(.Rows(intCounter).Item("id"))



                If .Rows(intCounter).Item("TitleWithoutQuotes") <> "" Then
                    oread = File.OpenText(Application.StartupPath + "\stop words.txt")
                    While oread.Peek <> -1
                        b.Add(oread.ReadLine.ToLower)
                    End While
                    oread.Close()
                    stri = ""
                    stri = .Rows(intCounter).Item("TitleWithoutQuotes").trim.ToLower
                    For index = 0 To punct.Length - 1 Step 1
                        stri = stri.Replace(punct(index), "")
                    Next
                    strtemp = ""
                    strtemp = stri
                    str = strtemp.Trim.Split(sep)
                    ' TextBox3.Text = stemTerm(strtemp)
                    ip = 0
                    irr = Nothing
                    ReDim irr(500)
                    For index = 0 To str.Length - 1 Step 1
                        If b.Contains(str(index)) Then
                            irr(ix) = str(index)
                            ix = ix + 1
                        Else
                            stemmedstr(ip) = stemTerm(str(index))
                            ip = ip + 1
                        End If
                    Next


                    'For indexx = 0 To ix - 1
                    '    ListBox1.Items.Add(IRR(indexx))
                    'Next
                    ix = 0
                    a = New List(Of String)
                    Do While ix < ip
                        'If Not a.Contains(stemmedstr(ix)) Then
                        a.Add(stemmedstr(ix))
                        'End If
                        ix = ix + 1
                    Loop
                    'keyw(500) = Nothing
                    keyw = Nothing
                    ReDim keyw(500)
                    'Array.Clear(keyw, 0, keyw.Length)
                    keyw = a.ToArray
                    Dim count(keyw.Length) As Integer
                    For indexx = 0 To keyw.Length - 1
                        For indexy = 0 To ip - 1
                            If keyw(indexx).Equals(stemmedstr(indexy)) Then
                                count(indexx) = count(indexx) + 1
                            End If
                        Next
                    Next
                    strStemmed = ""
                    li = Nothing
                    ReDim li(2)
                    'Array.Clear(li, 0, li.Length)
                    For indexx = 0 To keyw.Length - 1 Step 1
                        li(0) = keyw(indexx)
                        strStemmed = strStemmed + " " + keyw(indexx)
                        li(1) = count(indexx)
                        'itm = New ListViewItem(li)
                        'ListView1.Items.Add(itm)
                    Next
                    'Else
                    'MsgBox("You have missed the problem type (or) Subject (or) Problem Definition", MsgBoxStyle.OkOnly, "Missed something")
                End If


                sqlStr = "update paper set stemmedTitle='" & strStemmed & "' where id=" & .Rows(intCounter).Item("id")
                cmd = New SqlClient.SqlCommand(sqlStr, con)
                cmd.ExecuteNonQuery()
            Next
            Console.ReadLine()

        End With
        con.Close()
        MessageBox.Show("ready")
    End Sub
End Class
