namespace VeloCarbon_UI
{
    public partial class FormInputAktivitas : Form
    {
        public FormInputAktivitas()
        {
            InitializeComponent();
        }

        private void FormInputAktivitas_Load(object sender, EventArgs e)
        {
            cbKendaraan.Items.AddRange(new object[] { "Motor (Bensin)", "Jalan kaki", "Bus", "Kereta" });
            cbKendaraan.SelectedIndex = 0;
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            // Validasi sesuai PRD (NFR-03): angka tidak boleh kosong atau nol/negatif
            if (nudJarak.Value <= 0 || nudFrekuensi.Value <= 0)
            {
                MessageBox.Show("Jarak dan frekuensi harus lebih dari 0.", "Input tidak valid",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Aktivitas perjalanan berhasil disimpan.", "VeloCarbon",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbKendaraan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
