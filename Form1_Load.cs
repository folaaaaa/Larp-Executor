using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using XenoUI;

namespace larp_executor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ClientsWindow.Initialize(false);
        }

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetClients();

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern void Execute(byte[] script, int[] PIDs, int count);

        [DllImport("Xeno.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Attach();

        private List<int> GetReadyClientPIDs()
        {
            var pids = new List<int>();

            try
            {
                IntPtr clientsPtr = GetClients();
                if (clientsPtr == IntPtr.Zero) return pids;

                string clientsJson = Marshal.PtrToStringAnsi(clientsPtr);
                var clientsList = JsonConvert.DeserializeObject<List<List<object>>>(clientsJson);

                if (clientsList != null)
                {
                    foreach (var client in clientsList)
                    {
                        if (client.Count >= 4)
                        {
                            int pid = Convert.ToInt32(client[0]);
                            int state = Convert.ToInt32(client[3]);

                            if (state == 3) // ready state
                            {
                                pids.Add(pid);
                            }
                        }
                    }
                }
            }
            catch
            {
                // optional: log error
            }

            return pids;
        }

        public void ExecuteScriptOnClients(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                MessageBox.Show("Script is empty.", "Empty Script", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var clientPIDs = GetReadyClientPIDs();

            if (clientPIDs.Count == 0)
            {
                MessageBox.Show("No ready clients found.\n\nMake sure you've pressed Attach and waited for injection to complete.",
                    "No Ready Clients", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Execute(Encoding.UTF8.GetBytes(script + "\0"), clientPIDs.ToArray(), clientPIDs.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Script execution failed:\n{ex.Message}", "Execution Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Attach automatically on load
                Attach();

                // Optional: check for ready clients
                var readyClients = GetReadyClientPIDs();
                if (readyClients.Count == 0)
                {
                    MessageBox.Show(
                        "No ready clients detected on startup.\n\nMake sure clients are running and attached.",
                        "No Ready Clients",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                else
                {
                    MessageBox.Show(
                        $"Detected {readyClients.Count} ready client(s) on startup.",
                        "Clients Ready",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during form load:\n{ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Attach();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string script = richTextBox1.Text;
            ExecuteScriptOnClients(script);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("only harmful script");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string message = "Made by FLYRY | malware.fit";
            string title = "Credits";
            MessageBox.Show(message, title);
        }
    }
}