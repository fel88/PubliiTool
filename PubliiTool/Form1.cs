using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using HtmlAgilityPack;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;


namespace PubliiTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        List<Post> posts = new List<Post>();
        public void load()
        {
            var dir = Path.GetDirectoryName(dbPath);
            using var con = new SqliteConnection("Data Source=" + dbPath);

            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
            con.Open();
            posts = con.Query<Post>("SELECT * FROM Posts").ToList();
            var res = con.QueryFirst("select SQLITE_VERSION() AS Version");
            var json = JsonDocument.Parse(File.ReadAllText(pagesJsonPath));
            var len = json.RootElement.GetArrayLength();
            UpdateTree(json.RootElement, null);

            UpdatePostsList();
        }

        private void UpdatePostsList()
        {
            listView1.Items.Clear();
            foreach (var item in posts)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (!item.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                listView1.Items.Add(new ListViewItem(new string[] { item.Id.ToString(), item.Title, item.Slug, item.Text, item.Parent != null ? (item.Parent.Id + " (" + item.Parent.Title + ")") : "" }) { Tag = item });
            }
        }

        public void UpdateTree(JsonElement root, Post parent)
        {
            var len = root.GetArrayLength();
            for (int i = 0; i < len; i++)
            {
                var obj = root[i];
                var id = obj.GetProperty("id").GetInt32();

                var post = posts.FirstOrDefault(z => z.Id == id);
                if (post != null)
                    post.Parent = parent;

                UpdateTree(obj.GetProperty("subpages"), post);


            }
        }


        string dbPath;
        string pagesJsonPath;
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            var post = listView1.SelectedItems[0].Tag as Post;
            richTextBox1.Text = post.Text;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "sqlite (*.db, *.sqlite)|*.db;*.sqlite";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            dbPath = ofd.FileName;
            pagesJsonPath = Path.Combine(Path.GetDirectoryName(dbPath), "config", "pages.config.json");
            load();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            foreach (var item in posts)
            {
                UpdateBreadcrumbs(item);
            }
        }

        public void UpdateBreadcrumbs(Post item)
        {
            var p = item.Parent;
            List<string> links = new List<string>();
            links.Add(item.Title);
            while (p != null)
            {
                links.Add("<a href=\"#INTERNAL_LINK#/page/" + p.Id + "\">" + p.Title + "</a>");
                p = p.Parent;
            }
            StringBuilder sb = new StringBuilder();
            links.Reverse();
            var join = string.Join(" :: ", links);
            join = $"<div class='breadcrumbs'>{join}</div>";
            //detect old one menu and replace it if exist
            // 2. Load the web page using HtmlWeb
            HtmlWeb web = new HtmlWeb();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(item.Text);
            // 3. Check if the document loaded correctly
            if (doc.DocumentNode != null)
            {
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='breadcrumbs']");
                if (nodes != null && nodes.Any())
                {
                    foreach (var node in nodes)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                    item.Text = doc.DocumentNode.OuterHtml;
                }
            }


            item.Text = item.Text.Insert(0, join);

        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure to update {dbPath}?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using var con = new SqliteConnection("Data Source=" + dbPath);

            //SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
            con.Open();
            foreach (var item in posts)
            {
                con.Update<Post>(item);
            }

        }

        private void updateSelectedInDbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            if (MessageBox.Show($"Are you sure to update {dbPath}?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using var con = new SqliteConnection("Data Source=" + dbPath);
            con.Open();


            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var item = listView1.SelectedItems[i].Tag as Post;

                con.Update<Post>(item);
            }
        }

        string filter = "";
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            filter = textBox1.Text;
            UpdatePostsList();
        }

        private void updateBreadcrumbsForSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
                        

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                var item = listView1.SelectedItems[i].Tag as Post;
                UpdateBreadcrumbs(item);
            }
        }
    }
}
