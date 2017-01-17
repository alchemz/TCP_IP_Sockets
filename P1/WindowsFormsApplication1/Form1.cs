using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        //keep default name
        TcpListener mTcpListener;
        TcpClient mTcpClient;
        byte[] mRx;

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnStartListening_Click(object sender, EventArgs e)
        {
            IPAddress ipaddr;
            int nPort;

            if(!int.TryParse(tbPort.Text, out nPort))
            {
                nPort = 23000;
            }
            if(!IPAddress.TryParse(tbIPAddress.Text, out ipaddr))
            {
                MessageBox.Show("Invalid IP address supplied");
                return;
            }

            mTcpListener = new TcpListener(ipaddr, nPort);
            //call start method
            mTcpListener.Start();
            //almost ready
            mTcpListener.BeginAcceptTcpClient(onCompleteAcceptTcpClient, mTcpListener);


        }

        void onCompleteAcceptTcpClient(IAsyncResult iar)
        {
            //iar will have a member variable
            TcpListener tcpl = (TcpListener)iar.AsyncState;

            try { 
            //call method
               mTcpClient= tcpl.EndAcceptTcpClient(iar);
                printLine("Client Connected");
                
                mRx = new byte[512];
                mTcpClient.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromTCPClientStream, mTcpClient);
            }catch(Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        void onCompleteReadFromTCPClientStream(IAsyncResult iar)
        {
            TcpClient tcpc;//local
            int nCountReadBytes = 0;
            //string variable
            string strRecv;

             try
            {
                tcpc = (TcpClient)iar.AsyncState;
                nCountReadBytes = tcpc.GetStream().EndRead(iar);

                if(nCountReadBytes==0)
                {
                    MessageBox.Show("Client disconnected");
                    return;
                }

                strRecv = Encoding.ASCII.GetString(mRx, 0, nCountReadBytes);
                printLine(strRecv);

                mRx = new byte[512];
                tcpc.GetStream().BeginRead(mRx, 0, mRx.Length, onCompleteReadFromTCPClientStream, tcpc);

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void printLine(string _strPrint)
        {
            tbConsoleOutput.Invoke(new Action<string>(doInvoke), _strPrint);
        }

        public void doInvoke(string _strPrint)
        {
            tbConsoleOutput.Text = _strPrint + Environment.NewLine + tbConsoleOutput.Text;
        }

      
        private void onCompleteWriteClientStream(IAsyncResult iar)
        {
            try
            {
                TcpClient tcpc = (TcpClient)iar.AsyncState;
                tcpc.GetStream().EndWrite(iar);

            }catch(Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbIPAddress_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSend_Click_1(object sender, EventArgs e)
        {
         
            byte[] tx = new byte[512];
            if (string.IsNullOrEmpty(tbPayload.Text)) return;

            try
            {
                if (mTcpClient != null)
                {
                    if (mTcpClient.Client.Connected)
                    {
                        tx = Encoding.ASCII.GetBytes(tbPayload.Text);
                        mTcpClient.GetStream().BeginWrite(tx, 0, tx.Length, onCompleteReadFromTCPClientStream, mTcpClient);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

    }
}
}
