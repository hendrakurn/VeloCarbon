namespace VeloCarbon_UI
{
    public partial class FormDashboard : Form
    {
        public FormDashboard()
        {
            InitializeComponent();
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            // Data contoh agar tampilan terlihat seperti aplikasi jadi (belum terhubung database)
            cbPeriode.Items.AddRange(new object[] { "Agustus 2026", "September 2026", "Oktober 2026" });
            cbPeriode.SelectedIndex = 2;

            dgvKategori.Rows.Add("Mobilitas (Motor)", "42,50", "62%");
            dgvKategori.Rows.Add("Mobilitas (Angkutan umum)", "3,20", "5%");
            dgvKategori.Rows.Add("Listrik kos", "22,80", "33%");
        }

        private void btnInputAktivitas_Click(object sender, EventArgs e)
        {
            FormInputAktivitas form = new FormInputAktivitas();
            form.ShowDialog();
        }

        private void btnKeluar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblJudul_Click(object sender, EventArgs e)
        {

        }
    }
}
