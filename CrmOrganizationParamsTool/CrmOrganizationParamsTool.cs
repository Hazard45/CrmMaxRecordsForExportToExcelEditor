using System;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;

namespace CrmOrganizationParamsTool
{
    public partial class CrmOrganizationParamsTool : Form
    {
        private IOrganizationService service;
        private Entity organizationEntity;

        public CrmOrganizationParamsTool()
        {
            InitializeComponent();
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void TextBoxOrganizationServiceUrl_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxOrganizationServiceUrl.Text))
            {
                ButtonConnect.Enabled = false;
            }
            else
            {
                ButtonConnect.Enabled = true;
            }
        }

        private void ComboBoxParameterName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxParameter.SelectedIndex >= 0)
            {
                try
                {
                    var param = (OrganizationParam)ComboBoxParameter.SelectedItem;
                    TextBoxParameterType.Text = param.GetValueTypeString();
                    TextBoxParameterValue.Text = param.GetValueString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ButtonSetValue_Click(object sender, EventArgs e)
        {
            if (ComboBoxParameter.SelectedIndex >= 0)
            {
                var dialogResult = MessageBox.Show("Any changes of organization parameters are unmanaged and can harm the Dynamics CRM Organization functionality." + Environment.NewLine + Environment.NewLine +
                    "Are you sure you want to change this organization parameter value?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dialogResult == DialogResult.Yes)
                {
                    SetValue();
                }
            }
        }

        private void GetData(int selectedIndex)
        {
            ComboBoxParameter.Items.Clear();
            var query = new QueryExpression("organization");
            query.ColumnSet = new ColumnSet(true);
            var organizations = service.RetrieveMultiple(query);
            if (organizations.Entities.Count == 1)
            {
                organizationEntity = organizations.Entities[0];
                var parameters = organizationEntity.Attributes.ToList();
                foreach (var param in parameters.OrderBy(p => p.Key))
                {
                    var orgParam = new OrganizationParam(param.Key, param.Value);
                    ComboBoxParameter.Items.Add(orgParam);
                    if (ComboBoxParameter.Items.Count > selectedIndex)
                    {
                        ComboBoxParameter.SelectedIndex = selectedIndex;
                    }
                }
            }
            else
            {
                throw new Exception("There is no such organization");
            }
        }

        private async void Connect()
        {
            GroupBoxOrganizationParameters.Enabled = false;
            PictureBoxConnectionLoading.Visible = true;
            Loading(true);
            try
            {
                var uri = new Uri(TextBoxOrganizationServiceUrl.Text);
                var credentials = new ClientCredentials();
                if (!string.IsNullOrWhiteSpace(TextBoxUserName.Text))
                {
                    credentials.UserName.UserName = TextBoxUserName.Text;
                    credentials.UserName.Password = TextBoxPassword.Text;
                }
                else
                {
                    credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                }

                await Task.Factory.StartNew(() =>
                {
                    service = new OrganizationServiceProxy(uri, null, credentials, null);

                    GetData(0);
                    GroupBoxOrganizationParameters.Enabled = true;
                    ComboBoxParameter.DroppedDown = true;
                });
            }
            catch (Exception ex)
            {
                ComboBoxParameter.Items.Clear();
                TextBoxParameterType.Text = string.Empty;
                TextBoxParameterValue.Text = string.Empty;
                GroupBoxOrganizationParameters.Enabled = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Loading(false);
                PictureBoxConnectionLoading.Visible = false;
                PictureBoxParameterLoading.Visible = false;
            }
        }

        private async void SetValue()
        {
            PictureBoxParameterLoading.Visible = true;
            Loading(true);
            try
            {
                var param = (OrganizationParam)ComboBoxParameter.SelectedItem;
                param.SetValue(TextBoxParameterValue.Text.Trim());
                organizationEntity[param.Name] = param.Value;
                await Task.Factory.StartNew(() =>
                {
                    service.Update(organizationEntity);
                    GetData(ComboBoxParameter.SelectedIndex);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Loading(false);
                PictureBoxParameterLoading.Visible = false;
            }
        }

        private void Loading(bool activate)
        {
            ComboBoxParameter.Enabled = !activate;
            TextBoxParameterType.Enabled = !activate;
            TextBoxParameterValue.Enabled = !activate;
            ButtonSetValue.Enabled = !activate;

            TextBoxOrganizationServiceUrl.Enabled = !activate;
            TextBoxUserName.Enabled = !activate;
            TextBoxPassword.Enabled = !activate;
            ButtonConnect.Enabled = !activate;
        }
    }
}