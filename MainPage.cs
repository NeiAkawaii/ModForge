namespace ModForge;
public class MainPage : ContentPage {
  readonly DllService svc = new();
  List<string> all = new();
  readonly SearchBar search = new() { Placeholder = "ค้นหา class" };
  readonly CollectionView list = new() { HeightRequest = 150, SelectionMode = SelectionMode.Single };
  readonly Editor code = new() {
    FontFamily = "monospace", FontSize = 12,
    IsSpellCheckEnabled = false, IsTextPredictionEnabled = false,
    VerticalOptions = LayoutOptions.Fill };
  readonly Label status = new() { FontSize = 12, TextColor = Colors.Orange, MaxLines = 2 };
  string lastOut;

  Button B(string t, Func<Task> a) {
    var b = new Button { Text = t, Padding = new Thickness(4), FontSize = 13 };
    b.Clicked += async (_, _) => { try { await a(); } catch (Exception e) { status.Text = e.Message; } };
    return b;
  }

  static Grid Row(params View[] v) {
    var g = new Grid { ColumnSpacing = 6 };
    for (int i = 0; i < v.Length; i++) {
      g.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
      g.Add(v[i], i, 0);
    }
    return g;
  }

  public MainPage() {
    list.SelectionChanged += async (_, e) => {
      if (e.CurrentSelection.FirstOrDefault() is not string n) return;
      status.Text = "กำลังถอดโค้ด...";
      try { code.Text = await Task.Run(() => svc.Decompile(n)); status.Text = n; }
      catch (Exception ex) { code.Text = ex.ToString(); status.Text = "ถอดโค้ดไม่สำเร็จ"; }
    };
    search.TextChanged += (_, e) => list.ItemsSource = all
      .Where(x => x.Contains(e.NewTextValue ?? "", StringComparison.OrdinalIgnoreCase)).ToList();

    var open = B("1. เปิด .dll", async () => {
      var f = await FilePicker.PickAsync(); if (f == null) return;
      svc.DllPath = await DllService.CopyPicked(f, "work");
      all = svc.TypeNames(); list.ItemsSource = all; status.Text = $"โหลดแล้ว {all.Count} class"; });
    var toggle = B("ซ่อน/แสดงรายการ", () => {
      search.IsVisible = !search.IsVisible; list.IsVisible = search.IsVisible; return Task.CompletedTask; });
    var copy = B("คัดลอกโค้ด", async () => {
      await Clipboard.Default.SetTextAsync(code.Text ?? ""); status.Text = "คัดลอกแล้ว"; });
    var refs = B("2. เพิ่ม reference", async () => {
      var fs = await FilePicker.PickMultipleAsync(); if (fs == null) return;
      foreach (var f in fs) svc.Refs.Add(await DllService.CopyPicked(f, "refs"));
      status.Text = $"refs: {svc.Refs.Count}"; });
    var comp = B("3. Compile", () => {
      var r = svc.Compile(code.Text, "MyMod"); lastOut = r.outPath;
      status.Text = r.ok ? "สำเร็จ " + r.outPath : r.log; return Task.CompletedTask; });
    var share = B("4. ส่งออก DLL", async () => {
      if (lastOut != null) await Share.RequestAsync(new ShareFileRequest { File = new ShareFile(lastOut) }); });

    var root = new Grid { Padding = new Thickness(8), RowSpacing = 6 };
    for (int i = 0; i < 7; i++)
      root.RowDefinitions.Add(new RowDefinition(i == 4 ? GridLength.Star : GridLength.Auto));
    root.Add(Row(open, toggle), 0, 0);
    root.Add(status, 0, 1);
    root.Add(search, 0, 2);
    root.Add(list, 0, 3);
    root.Add(code, 0, 4);
    root.Add(Row(copy, refs), 0, 5);
    root.Add(Row(comp, share), 0, 6);
    Content = root;
  }
}
