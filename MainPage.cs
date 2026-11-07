namespace ModForge;
public class MainPage : ContentPage {
  readonly DllService svc = new();
  List<string> all = new();
  readonly SearchBar search = new() { Placeholder = "ค้นหา class" };
  readonly CollectionView list = new() { HeightRequest = 220, SelectionMode = SelectionMode.Single };
  readonly Editor code = new() { FontFamily = "monospace", FontSize = 12, HeightRequest = 380 };
  readonly Label status = new() { FontSize = 12, TextColor = Colors.Orange };
  string lastOut;
  Button B(string t, Func<Task> a) { var b = new Button { Text = t }; b.Clicked += async (_, _) => { try { await a(); } catch (Exception e) { status.Text = e.Message; } }; return b; }
  public MainPage() {
    list.SelectionChanged += async (_, e) => {
      if (e.CurrentSelection.FirstOrDefault() is not string n) return;
      status.Text = "กำลังถอดโค้ด...";
      try { code.Text = await Task.Run(() => svc.Decompile(n)); status.Text = n; }
      catch (Exception ex) { code.Text = ex.ToString(); status.Text = "ถอดโค้ดไม่สำเร็จ"; }
    };
    search.TextChanged += (_, e) => list.ItemsSource = all
      .Where(x => x.Contains(e.NewTextValue ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
    Content = new ScrollView { Content = new VerticalStackLayout { Padding = 12, Spacing = 8, Children = {
      B("1. เปิดไฟล์ .dll", async () => { var f = await FilePicker.PickAsync(); if (f == null) return;
        svc.DllPath = await DllService.CopyPicked(f, "work"); all = svc.TypeNames(); list.ItemsSource = all; status.Text = $"โหลดแล้ว {all.Count} class"; }),
      status,
      search, list, code,
      B("คัดลอกโค้ดทั้งหมด", async () => { await Clipboard.Default.SetTextAsync(code.Text ?? ""); status.Text = "คัดลอกแล้ว"; }),
      B("2. เพิ่ม reference (เลือกหลายไฟล์)", async () => { var fs = await FilePicker.PickMultipleAsync();
        if (fs == null) return; foreach (var f in fs) svc.Refs.Add(await DllService.CopyPicked(f, "refs")); status.Text = $"refs: {svc.Refs.Count}"; }),
      B("3. Compile เป็น DLL", async () => { var r = svc.Compile(code.Text, "MyMod"); lastOut = r.outPath;
        status.Text = r.ok ? "สำเร็จ " + r.outPath : r.log; await Task.CompletedTask; }),
      B("4. ส่งออกไฟล์ที่ compile", async () => { if (lastOut != null) await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(lastOut) }); }) } } };
  }
}
