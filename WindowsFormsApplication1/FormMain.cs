using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Web;
using System.IO;
using System.Net;


using System.Globalization;
using System.Net.Configuration;

namespace IManXPathFinderForNet
{
    /// <summary>
    /// For those who want to extract information from an Html page using .Net, it is sometimes painful
    /// when you try to get a node or an attribute by XPath. Tools such as Firebug addon for Firefox 
    /// get you the different XPath format that you could not use in a .Net language. This application provides you that option
    /// for instance: the Firebug format:       /html/body/div[2]/div[2]/section/div/article/section
    /// the .Net format:                        /html[1]/body[1]/div[2]/div[2]/section[1]/div[1]/article[1]/section[1]/h1[1]/#text[1]
    /// </summary>
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            document = new HtmlAgilityPack.HtmlDocument();
            webClient = new WebClient();
            nodeCollection = new List<HtmlAgilityPack.HtmlNode>();
            attributeCollection = new List<HtmlAgilityPack.HtmlAttribute>();
            this.lblStat.Text = string.Empty;
        }


        HtmlAgilityPack.HtmlDocument document;
        WebClient webClient;
        
        List<HtmlAgilityPack.HtmlNode> nodeCollection;
        List<HtmlAgilityPack.HtmlAttribute> attributeCollection;
        private void btnGo_Click(object sender, EventArgs e)
        {
            LoadDocumentFromWeb();
        }

        /// <summary>
        /// Access to a website and load page
        /// </summary>
        private void LoadDocumentFromWeb()
        {
            this.tabControl1.SelectedTab = this.tabPage1;

            if (this.txtUrl.Text == null || this.txtUrl.Text.Length < 10)
            {
                MessageBox.Show("Invalid url");
                return;
            }
            try
            {
                var u = new UriBuilder(this.txtUrl.Text).Uri;
                this.txtUrl.Text = u.AbsoluteUri;
                var pageData = webClient.DownloadData(u);
                var html = Encoding.UTF8.GetString(pageData);
                this.txtHtml.Text = html;
                this.btnParse.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Load the Aligity pack 
        /// </summary>
        void parseDocument()
        {
            if (this.txtHtml.Text == string.Empty)
            {
                MessageBox.Show("Html text should not be empty");
                return;
            }
            document.LoadHtml(this.txtHtml.Text);
            this.nodeCollection.Clear();
            this.nodeCollection.AddRange(document.DocumentNode.Descendants());

            this.attributeCollection.Clear();
            foreach (var node in nodeCollection)
            {
                this.attributeCollection.AddRange(node.Attributes);
            }

            this.txtStatus.Text = string.Format("Number of Nodes :{0} - Number of Attributes: {1}", 
                nodeCollection.Count, attributeCollection.Count);
            this.listboxXpath.Items.Clear();
            this.listboxXpath.Items.Add(this.txtStatus.Text);
            
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoSearchForXPath(this.txtSearch.Text.Trim());

        }

        /// <summary>
        /// Search for nodes and attributes that have text or value contain the textToSearch
        /// </summary>
        /// <param name="textToSearch">Look for the text</param>
        private void DoSearchForXPath(string textToSearch)
        {
            if (textToSearch == string.Empty) return;
            if (textToSearch != null && textToSearch.Length < 2)
            {
                MessageBox.Show("please input search string");
                return;
            }
            listboxXpath.Items.Clear();
            var result = attributeCollection.Where(a => a.Value.Contains(textToSearch)).ToList();
            foreach (var item in result)
            {
                this.listboxXpath.Items.Add(item.XPath);
            }

            var result1 = this.nodeCollection.Where(a => a.InnerText.Contains(textToSearch)).ToList();
            foreach (var item in result1)
            {
                this.listboxXpath.Items.Add(item.XPath);
            }
            this.listboxXpath.Select();
        }

        private void DoSearchForXPath()
        {
            if (this.txtSearch.Text == string.Empty) return;
            if (this.txtSearch.Text != null && this.txtSearch.Text.Length < 2)
            {
                MessageBox.Show("please input search string");
                return;
            }
            listboxXpath.Items.Clear();
            var result = attributeCollection.Where(a => a.Value.Contains(this.txtSearch.Text)).ToList();
            foreach (var item in result)
            {
                this.listboxXpath.Items.Add(item.XPath);
            }

            var result1 = this.nodeCollection.Where(a => a.InnerText.Contains(this.txtSearch.Text)).ToList();
            foreach (var item in result1)
            {
                this.listboxXpath.Items.Add(item.XPath);
            }
            this.listboxXpath.Select();
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            this.parseDocument();
            this.tabControl1.SelectedTab = this.tabPage2;
            //this.txtSearch.Select();
        }

        private void listboxXpath_SelectedIndexChanged(object sender, EventArgs e)
        {
            //When listbox item changed, then show the properties of according node or html attribute
            //string parentNodeXPath = string.Empty;
            if (listboxXpath.SelectedItem != null)
            {
                var o = this.attributeCollection.Where(a => a.XPath == this.listboxXpath.SelectedItem.ToString()).FirstOrDefault();
                if (o != null)
                {
                    this.propertyGridElement.SelectedObject = o;
                    //parentNodeXPath = o.OwnerNode.XPath;
                }
                else
                {
                    var o1 = this.nodeCollection.Where(a => a.XPath == this.listboxXpath.SelectedItem.ToString()).FirstOrDefault();
                    this.propertyGridElement.SelectedObject = o1;
                    //if(o1.ParentNode != null) parentNodeXPath = o1.ParentNode.XPath;
                }
                //MessageBox.Show(parentNodeXPath);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            // load the page from web when user hits enter
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                this.LoadDocumentFromWeb();
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            //if user hit enter when the search box is selected, then do the search
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                this.DoSearchForXPath(this.txtSearch.Text.Trim());
            }
        }

        private void listboxXpath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control)// Copy xpath to cliboard
            {
                System.Windows.Forms.Clipboard.SetText(this.listboxXpath.SelectedItem.ToString());
                //MessageBox.Show("Selected XPath coppied to cliboard!");
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("vietvt@gmail.com");
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    //public class ExHtmlNode: HtmlAgilityPack.HtmlNode
    //{
    //    public string ParentNodeXpath
    //    {
    //        get
    //        {
    //            if (this.ParentNode!= null)
    //            {
    //                return this.ParentNode.XPath;
    //            }
    //            return null;
    //        }
    //    }
    //}
    //public class ExHtmlAttribute : HtmlAgilityPack.HtmlAttribute
    //{
    //    public string ParentNodeXpath
    //    {
    //        get
    //        {
    //            if (this.OwnerNode != null)
    //            {
    //                return this.OwnerNode.XPath;
    //            }
    //            return null;
    //        }
    //    }
    //}
}
