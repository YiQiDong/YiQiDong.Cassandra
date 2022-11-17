using WinNodetool.Core;
using WinNodetool.Utils;

namespace WinNodetool
{
    public partial class MainForm : Form
    {
        private ConfigModel config;
        public MainForm()
        {
            InitializeComponent();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            Text += "_" + Application.ProductVersion;
            config = ConfigFileUtils.Load<ConfigModel>();
            if (config == null)
                config = new ConfigModel();
            txtCassandraImageDir.Text = config.ImageDir;
            txtCassandraContainerDir.Text = config.ContainerDir;
        }

        private Queue<string> logQueue = new Queue<string>();
        private void pushLog(string log)
        {
            this.BeginInvoke(new Action(() =>
            {
                logQueue.Enqueue(log);
                //最多显示1000行日志
                while (logQueue.Count > 1000)
                    logQueue.Dequeue();
                txtLogs.Lines = logQueue.ToArray();
                txtLogs.Select(txtLogs.TextLength, 0);
                txtLogs.ScrollToCaret();
            }));
        }

        private void btnSelectCassandraImageDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var ret = fbd.ShowDialog();
            if (ret == DialogResult.Cancel)
                return;
            txtCassandraImageDir.Text = fbd.SelectedPath;
        }

        private void btnSelectCassandraContainerDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var ret = fbd.ShowDialog();
            if (ret == DialogResult.Cancel)
                return;
            txtCassandraContainerDir.Text = fbd.SelectedPath;
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                gbOperate.Enabled = false;

                var imageDir = txtCassandraImageDir.Text.Trim();
                var containerDir = txtCassandraContainerDir.Text.Trim();

                var isConfigChanged = imageDir != config.ImageDir || containerDir != config.ContainerDir;
                if (isConfigChanged)
                {
                    config.ImageDir = imageDir;
                    config.ContainerDir = containerDir;
                    ConfigFileUtils.Save(config);
                }

                var arguments = txtArguments.Text.Trim();
                var argumentsSegments = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                pushLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 开始执行命令：nodetool {arguments}");

                await Task.Run(() =>
                {
                    CassandraUtils.RunNodeTool(
                    imageDir,
                    containerDir,
                    pushLog,
                    argumentsSegments);
                    pushLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 执行命令完成：nodetool {arguments}");
                });
            }
            catch (Exception ex)
            {
                pushLog($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 运行时出错，原因：{ex}");
            }
            finally
            {
                gbOperate.Enabled = true;
            }
        }
    }
}