﻿using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using QLSVNhomApp.Data;

namespace QLSVNhomApp.Forms
{
    public partial class InputScoreForm : Form
    {
        private string connectionString;
        private string loggedInEmployeeID;
        private string classID;

        public InputScoreForm(string connStr, string empID, string classId)
        {
            connectionString = connStr;
            loggedInEmployeeID = empID;
            classID = classId;
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            string maSV = txtMaSV.Text.Trim();
            string maHP = txtMaHP.Text.Trim();
            string diemStr = txtDiem.Text.Trim();

            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(maHP) || string.IsNullOrEmpty(diemStr))
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Vui lòng nhập đầy đủ thông tin.";
                return;
            }

            if (!decimal.TryParse(diemStr, out decimal diem) || diem < 0 || diem > 10)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Điểm không hợp lệ (0 - 10).";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand checkCmd = new SqlCommand("SP_FIND_STUDENT_IN_CLASS", conn))
                    {
                        checkCmd.CommandType = CommandType.StoredProcedure;
                        checkCmd.Parameters.AddWithValue("@MASV", maSV);
                        checkCmd.Parameters.AddWithValue("@MALOP", classID); 

                        SqlDataAdapter adapter = new SqlDataAdapter(checkCmd);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count == 0)  
                        {
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            lblMessage.Text = "Sinh viên không thuộc lớp này. Không thể nhập điểm!";
                            return;
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("SP_INS_DIEM", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MASV", maSV);
                        cmd.Parameters.AddWithValue("@MAHP", maHP);
                        cmd.Parameters.AddWithValue("@DIEMTHI", diem);
                        cmd.Parameters.AddWithValue("@PUBKEY", loggedInEmployeeID);
                        SqlParameter outError = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 200)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outError);

                        cmd.ExecuteNonQuery();

                        string errorMsg = outError.Value.ToString();
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            lblMessage.Text = errorMsg;
                        }
                        else
                        {
                            lblMessage.ForeColor = System.Drawing.Color.Green;
                            lblMessage.Text = "Nhập điểm thành công!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Lỗi: " + ex.Message;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
