namespace VeloCarbon_UI
{
    partial class FormInputAktivitas
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
            gbPerjalanan = new GroupBox();
            lblKendaraan = new Label();
            cbKendaraan = new ComboBox();
            lblTanggal = new Label();
            dtpTanggal = new DateTimePicker();
            lblJarak = new Label();
            nudJarak = new NumericUpDown();
            lblJarakSatuan = new Label();
            lblFrekuensi = new Label();
            nudFrekuensi = new NumericUpDown();
            lblFrekuensiSatuan = new Label();
            btnSimpan = new Button();
            btnBatal = new Button();
            gbPerjalanan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudJarak).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFrekuensi).BeginInit();
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
            lblJudul.Size = new Size(683, 72);
            lblJudul.TabIndex = 0;
            lblJudul.Text = "Input Aktivitas Perjalanan";
            // 
            // gbPerjalanan
            // 
            gbPerjalanan.Controls.Add(lblKendaraan);
            gbPerjalanan.Controls.Add(cbKendaraan);
            gbPerjalanan.Controls.Add(lblTanggal);
            gbPerjalanan.Controls.Add(dtpTanggal);
            gbPerjalanan.Controls.Add(lblJarak);
            gbPerjalanan.Controls.Add(nudJarak);
            gbPerjalanan.Controls.Add(lblJarakSatuan);
            gbPerjalanan.Controls.Add(lblFrekuensi);
            gbPerjalanan.Controls.Add(nudFrekuensi);
            gbPerjalanan.Controls.Add(lblFrekuensiSatuan);
            gbPerjalanan.Location = new Point(42, 133);
            gbPerjalanan.Margin = new Padding(6, 6, 6, 6);
            gbPerjalanan.Name = "gbPerjalanan";
            gbPerjalanan.Padding = new Padding(6, 6, 6, 6);
            gbPerjalanan.Size = new Size(914, 410);
            gbPerjalanan.TabIndex = 1;
            gbPerjalanan.TabStop = false;
            gbPerjalanan.Text = "Data Perjalanan";
            // 
            // lblKendaraan
            // 
            lblKendaraan.AutoSize = true;
            lblKendaraan.Location = new Point(32, 76);
            lblKendaraan.Margin = new Padding(6, 0, 6, 0);
            lblKendaraan.Name = "lblKendaraan";
            lblKendaraan.Size = new Size(263, 41);
            lblKendaraan.TabIndex = 0;
            lblKendaraan.Text = "Kendaraan/Moda :";
            // 
            // cbKendaraan
            // 
            cbKendaraan.DropDownStyle = ComboBoxStyle.DropDownList;
            cbKendaraan.Location = new Point(340, 68);
            cbKendaraan.Margin = new Padding(6, 6, 6, 6);
            cbKendaraan.Name = "cbKendaraan";
            cbKendaraan.Size = new Size(527, 49);
            cbKendaraan.TabIndex = 1;
            cbKendaraan.SelectedIndexChanged += cbKendaraan_SelectedIndexChanged;
            // 
            // lblTanggal
            // 
            lblTanggal.AutoSize = true;
            lblTanggal.Location = new Point(32, 158);
            lblTanggal.Margin = new Padding(6, 0, 6, 0);
            lblTanggal.Name = "lblTanggal";
            lblTanggal.Size = new Size(136, 41);
            lblTanggal.TabIndex = 2;
            lblTanggal.Text = "Tanggal :";
            // 
            // dtpTanggal
            // 
            dtpTanggal.Format = DateTimePickerFormat.Short;
            dtpTanggal.Location = new Point(340, 150);
            dtpTanggal.Margin = new Padding(6, 6, 6, 6);
            dtpTanggal.Name = "dtpTanggal";
            dtpTanggal.Size = new Size(527, 47);
            dtpTanggal.TabIndex = 3;
            // 
            // lblJarak
            // 
            lblJarak.AutoSize = true;
            lblJarak.Location = new Point(32, 240);
            lblJarak.Margin = new Padding(6, 0, 6, 0);
            lblJarak.Name = "lblJarak";
            lblJarak.Size = new Size(99, 41);
            lblJarak.TabIndex = 4;
            lblJarak.Text = "Jarak :";
            // 
            // nudJarak
            // 
            nudJarak.DecimalPlaces = 2;
            nudJarak.Location = new Point(340, 232);
            nudJarak.Margin = new Padding(6, 6, 6, 6);
            nudJarak.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            nudJarak.Name = "nudJarak";
            nudJarak.Size = new Size(319, 47);
            nudJarak.TabIndex = 5;
            // 
            // lblJarakSatuan
            // 
            lblJarakSatuan.AutoSize = true;
            lblJarakSatuan.Location = new Point(680, 240);
            lblJarakSatuan.Margin = new Padding(6, 0, 6, 0);
            lblJarakSatuan.Name = "lblJarakSatuan";
            lblJarakSatuan.Size = new Size(59, 41);
            lblJarakSatuan.TabIndex = 6;
            lblJarakSatuan.Text = "km";
            // 
            // lblFrekuensi
            // 
            lblFrekuensi.AutoSize = true;
            lblFrekuensi.Location = new Point(32, 322);
            lblFrekuensi.Margin = new Padding(6, 0, 6, 0);
            lblFrekuensi.Name = "lblFrekuensi";
            lblFrekuensi.Size = new Size(159, 41);
            lblFrekuensi.TabIndex = 7;
            lblFrekuensi.Text = "Frekuensi :";
            // 
            // nudFrekuensi
            // 
            nudFrekuensi.Location = new Point(340, 314);
            nudFrekuensi.Margin = new Padding(6, 6, 6, 6);
            nudFrekuensi.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudFrekuensi.Name = "nudFrekuensi";
            nudFrekuensi.Size = new Size(319, 47);
            nudFrekuensi.TabIndex = 8;
            nudFrekuensi.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblFrekuensiSatuan
            // 
            lblFrekuensiSatuan.AutoSize = true;
            lblFrekuensiSatuan.Location = new Point(680, 322);
            lblFrekuensiSatuan.Margin = new Padding(6, 0, 6, 0);
            lblFrekuensiSatuan.Name = "lblFrekuensiSatuan";
            lblFrekuensiSatuan.Size = new Size(62, 41);
            lblFrekuensiSatuan.TabIndex = 9;
            lblFrekuensiSatuan.Text = "kali";
            // 
            // btnSimpan
            // 
            btnSimpan.BackColor = Color.FromArgb(34, 120, 74);
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Location = new Point(446, 584);
            btnSimpan.Margin = new Padding(6, 6, 6, 6);
            btnSimpan.Name = "btnSimpan";
            btnSimpan.Size = new Size(244, 78);
            btnSimpan.TabIndex = 2;
            btnSimpan.Text = "Simpan";
            btnSimpan.UseVisualStyleBackColor = false;
            btnSimpan.Click += btnSimpan_Click;
            // 
            // btnBatal
            // 
            btnBatal.Location = new Point(712, 584);
            btnBatal.Margin = new Padding(6, 6, 6, 6);
            btnBatal.Name = "btnBatal";
            btnBatal.Size = new Size(244, 78);
            btnBatal.TabIndex = 3;
            btnBatal.Text = "Batal";
            btnBatal.UseVisualStyleBackColor = true;
            btnBatal.Click += btnBatal_Click;
            // 
            // FormInputAktivitas
            // 
            AutoScaleDimensions = new SizeF(17F, 41F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1007, 707);
            Controls.Add(lblJudul);
            Controls.Add(gbPerjalanan);
            Controls.Add(btnSimpan);
            Controls.Add(btnBatal);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormInputAktivitas";
            StartPosition = FormStartPosition.CenterParent;
            Text = "VeloCarbon - Input Aktivitas";
            Load += FormInputAktivitas_Load;
            gbPerjalanan.ResumeLayout(false);
            gbPerjalanan.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudJarak).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFrekuensi).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblJudul;
        private GroupBox gbPerjalanan;
        private Label lblKendaraan;
        private ComboBox cbKendaraan;
        private Label lblTanggal;
        private DateTimePicker dtpTanggal;
        private Label lblJarak;
        private NumericUpDown nudJarak;
        private Label lblJarakSatuan;
        private Label lblFrekuensi;
        private NumericUpDown nudFrekuensi;
        private Label lblFrekuensiSatuan;
        private Button btnSimpan;
        private Button btnBatal;
    }
}
