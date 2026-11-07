namespace ModForge;
public class MainPage : ContentPage {
  readonly DllService svc = new();
  readonly CollectionView list = new() { HeightRequest = 160 };
  readonly Editor code = new() { FontFamily = "monospace", FontSize = 12, HeightRequest = 380 };
  readonly Label status = new() { FontSize = 12 };
  string lastOut;
  Button B(string t, Func<Task> a) { var b = new Button { Text = t }; b.Clicked += async (_, _) => { try { await a(); } catch (Exception e) { status.Text = e.Message; } }; return b; }
  public MainPage() {
    list.SelectionChanged += (_, e) => { if (e.CurrentSelection.FirstOrDefault() is string n) code.Text = svc.Decompile(n); };
    Content = new ScrollView { Content = new VerticalStackLayout { Padding = 12, Spacing = 8, Children = {
      B("1. เปิดไฟล์ .dll", async () => { var f = await FilePicker.PickAsync(); if (f == null) return;
        svc.DllPath = await DllService.CopyPicked(f, "work"); list.ItemsSource = svc.TypeNames(); status.Text = "โหลดแล้ว"; }),
      list, code,
      B("2. เพิ่ม reference (เลือกหลายไฟล์)", async () => { var fs = await FilePicker.PickMultipleAsync();
        if (fs == null) return; foreach (var f in fs) svc.Refs.Add(await DllService.CopyPicked(f, "refs")); status.Text = $"refs: {svc.Refs.Count}"; }),
      B("3. Compile เป็น DLL", async () => { var r = svc.Compile(code.Text, "MyMod"); lastOut = r.outPath;
        status.Text = r.ok ? "สำเร็จ " + r.outPath : r.log; await Task.CompletedTask; }),
      B("4. ส่งออกไฟล์ที่ compile", async () => { if (lastOut != null) await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(lastOut) }); }),
      status } } };
  }
}
