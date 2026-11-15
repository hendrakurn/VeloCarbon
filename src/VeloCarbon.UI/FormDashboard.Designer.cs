namespace VeloCarbon_UI
{
    partial class FormDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblJudul = new Label();
            lblPeriode = new Label();
            cbPeriode = new ComboBox();
            gbRingkasan = new GroupBox();
            lblTotalNilai = new Label();
            lblTotalJudul = new Label();
            lblSumberNilai = new Label();
            lblSumberJudul = new Label();
            dgvKategori = new DataGridView();
            colKategori = new DataGridViewTextBoxColumn();
            colEmisi = new DataGridViewTextBoxColumn();
            colPersen = new DataGridViewTextBoxColumn();
            lblKategori = new Label();
            btnInputAktivitas = new Button();
            btnKeluar = new Button();
            gbRingkasan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKategori).BeginInit();
            SuspendLayout();
            // 
            // lblJudul
            // 
            lblJudul.AutoSize = true;
            lblJudul.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblJudul.ForeColor = Color.FromArgb(34, 120, 74);
            lblJudul.Location = new Point(42, 31);
            lblJudul.Margin = new Padding(6, 0, 6, 0);
            lblJudul.Name = "lblJudul";
            lblJudul.Size = new Size(651, 72);
            lblJudul.TabIndex = 0;
            lblJudul.Text = "VeloCarbon [Dashboard]";
            lblJudul.Click += lblJudul_Click;
            // 
            // lblPeriode
            // 
            lblPeriode.AutoSize = true;
            lblPeriode.Location = new Point(42, 144);
            lblPeriode.Margin = new Padding(6, 0, 6, 0);
            lblPeriode.Name = "lblPeriode";
            lblPeriode.Size = new Size(134, 41);
            lblPeriode.TabIndex = 1;
            lblPeriode.Text = "Periode :";
            // 
            // cbPeriode
            // 
            cbPeriode.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPeriode.Location = new Point(202, 135);
            cbPeriode.Margin = new Padding(6);
            cbPeriode.Name = "cbPeriode";
            cbPeriode.Size = new Size(378, 49);
            cbPeriode.TabIndex = 2;
            // 
            // gbRingkasan
            // 
            gbRingkasan.Controls.Add(lblTotalNilai);
            gbRingkasan.Controls.Add(lblTotalJudul);
            gbRingkasan.Controls.Add(lblSumberNilai);
            gbRingkasan.Controls.Add(lblSumberJudul);
            gbRingkasan.Location = new Point(42, 215);
            gbRingkasan.Margin = new Padding(6);
            gbRingkasan.Name = "gbRingkasan";
            gbRingkasan.Padding = new Padding(6);
            gbRingkasan.Size = new Size(1232, 184);
            gbRingkasan.TabIndex = 3;
            gbRingkasan.TabStop = false;
            gbRingkasan.Text = "Ringkasan Bulan Ini";
            // 
            // lblTotalNilai
            // 
            lblTotalNilai.AutoSize = true;
            lblTotalNilai.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTotalNilai.Location = new Point(319, 57);
            lblTotalNilai.Margin = new Padding(6, 0, 6, 0);
            lblTotalNilai.Name = "lblTotalNilai";
            lblTotalNilai.Size = new Size(261, 50);
            lblTotalNilai.TabIndex = 0;
            lblTotalNilai.Text = "68,50 kgCO2e";
            // 
            // lblTotalJudul
            // 
            lblTotalJudul.AutoSize = true;
            lblTotalJudul.Location = new Point(32, 62);
            lblTotalJudul.Margin = new Padding(6, 0, 6, 0);
            lblTotalJudul.Name = "lblTotalJudul";
            lblTotalJudul.Size = new Size(173, 41);
            lblTotalJudul.TabIndex = 1;
            lblTotalJudul.Text = "Total emisi :";
            // 
            // lblSumberNilai
            // 
            lblSumberNilai.AutoSize = true;
            lblSumberNilai.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSumberNilai.Location = new Point(319, 117);
            lblSumberNilai.Margin = new Padding(6, 0, 6, 0);
            lblSumberNilai.Name = "lblSumberNilai";
            lblSumberNilai.Size = new Size(305, 46);
            lblSumberNilai.TabIndex = 2;
            lblSumberNilai.Text = "Mobilitas - Motor";
            // 
            // lblSumberJudul
            // 
            lblSumberJudul.AutoSize = true;
            lblSumberJudul.Location = new Point(32, 119);
            lblSumberJudul.Margin = new Padding(6, 0, 6, 0);
            lblSumberJudul.Name = "lblSumberJudul";
            lblSumberJudul.Size = new Size(252, 41);
            lblSumberJudul.TabIndex = 3;
            lblSumberJudul.Text = "Sumber terbesar :";
            // 
            // dgvKategori
            // 
            dgvKategori.AllowUserToAddRows = false;
            dgvKategori.AllowUserToDeleteRows = false;
            dgvKategori.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKategori.BackgroundColor = SystemColors.Window;
            dgvKategori.ColumnHeadersHeight = 58;
            dgvKategori.Columns.AddRange(new DataGridViewColumn[] { colKategori, colEmisi, colPersen });
            dgvKategori.Location = new Point(42, 482);
            dgvKategori.Margin = new Padding(6);
            dgvKategori.Name = "dgvKategori";
            dgvKategori.ReadOnly = true;
            dgvKategori.RowHeadersVisible = false;
            dgvKategori.RowHeadersWidth = 51;
            dgvKategori.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKategori.Size = new Size(1232, 266);
            dgvKategori.TabIndex = 5;
            // 
            // colKategori
            // 
            colKategori.HeaderText = "Kategori";
            colKategori.MinimumWidth = 6;
            colKategori.Name = "colKategori";
            colKategori.ReadOnly = true;
            // 
            // colEmisi
            // 
            colEmisi.HeaderText = "Emisi (kgCO2e)";
            colEmisi.MinimumWidth = 6;
            colEmisi.Name = "colEmisi";
            colEmisi.ReadOnly = true;
            // 
            // colPersen
            // 
            colPersen.HeaderText = "Kontribusi";
            colPersen.MinimumWidth = 6;
            colPersen.Name = "colPersen";
            colPersen.ReadOnly = true;
            // 
            // lblKategori
            // 
            lblKategori.AutoSize = true;
            lblKategori.Location = new Point(42, 430);
            lblKategori.Margin = new Padding(6, 0, 6, 0);
            lblKategori.Name = "lblKategori";
            lblKategori.Size = new Size(320, 41);
            lblKategori.TabIndex = 4;
            lblKategori.Text = "Kontribusi per kategori";
            // 
            // btnInputAktivitas
            // 
            btnInputAktivitas.BackColor = Color.FromArgb(34, 120, 74);
            btnInputAktivitas.FlatStyle = FlatStyle.Flat;
            btnInputAktivitas.ForeColor = Color.White;
            btnInputAktivitas.Location = new Point(42, 779);
            btnInputAktivitas.Margin = new Padding(6);
            btnInputAktivitas.Name = "btnInputAktivitas";
            btnInputAktivitas.Size = new Size(361, 78);
            btnInputAktivitas.TabIndex = 6;
            btnInputAktivitas.Text = "Input Aktivitas";
            btnInputAktivitas.UseVisualStyleBackColor = false;
            btnInputAktivitas.Click += btnInputAktivitas_Click;
            // 
            // btnKeluar
            // 
            btnKeluar.Location = new Point(1020, 779);
            btnKeluar.Margin = new Padding(6);
            btnKeluar.Name = "btnKeluar";
            btnKeluar.Size = new Size(255, 78);
            btnKeluar.TabIndex = 7;
            btnKeluar.Text = "Keluar";
            btnKeluar.UseVisualStyleBackColor = true;
            btnKeluar.Click += btnKeluar_Click;
            // 
            // FormDashboard
            // 
            AutoScaleDimensions = new SizeF(17F, 41F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1326, 904);
            Controls.Add(lblJudul);
            Controls.Add(lblPeriode);
            Controls.Add(cbPeriode);
            Controls.Add(gbRingkasan);
            Controls.Add(lblKategori);
            Controls.Add(dgvKategori);
            Controls.Add(btnInputAktivitas);
            Controls.Add(btnKeluar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(6);
            MaximizeBox = false;
            Name = "FormDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "VeloCarbon - Dashboard";
            Load += FormDashboard_Load;
            gbRingkasan.ResumeLayout(false);
            gbRingkasan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKategori).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblJudul;
        private Label lblPeriode;
        private ComboBox cbPeriode;
        private GroupBox gbRingkasan;
        private Label lblTotalJudul;
        private Label lblTotalNilai;
        private Label lblSumberJudul;
        private Label lblSumberNilai;
        private Label lblKategori;
        private DataGridView dgvKategori;
        private DataGridViewTextBoxColumn colKategori;
        private DataGridViewTextBoxColumn colEmisi;
        private DataGridViewTextBoxColumn colPersen;
        private Button btnInputAktivitas;
        private Button btnKeluar;
    }
}
