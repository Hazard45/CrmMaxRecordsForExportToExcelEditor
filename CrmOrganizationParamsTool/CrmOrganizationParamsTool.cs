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

                service = new OrganizationServiceProxy(uri, null, credentials, null);

                GroupBoxOrganizationParameters.Enabled = true;
                GetData(0);
                ComboBoxParameter.DroppedDown = true;
            }
            catch (Exception ex)
            {
                ComboBoxParameter.Items.Clear();
                TextBoxParameterType.Text = string.Empty;
                TextBoxParameterValue.Text = string.Empty;
                GroupBoxOrganizationParameters.Enabled = false;
                PictureBoxLoading.Visible = false;
                MessageBox.Show(ex.ToString(), "Error");
            }
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
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }
        }

        private void ButtonSetValue_Click(object sender, EventArgs e)
        {
            if (ComboBoxParameter.SelectedIndex >= 0)
            {
                SetValue();
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

        private async void SetValue()
        {
            try
            {
                LoadingStarting();
                var param = (OrganizationParam)ComboBoxParameter.SelectedItem;
                param.SetValue(TextBoxParameterValue.Text.Trim());
                organizationEntity[param.Name] = param.Value;
                await Task.Factory.StartNew(() =>
                {
                    service.Update(organizationEntity);
                    GetData(ComboBoxParameter.SelectedIndex);
                });
                LoadingFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void LoadingStarting()
        {
            PictureBoxLoading.Visible = true;
            ComboBoxParameter.Enabled = false;
            TextBoxParameterType.Enabled = false;
            TextBoxParameterValue.Enabled = false;
            ButtonSetValue.Enabled = false;
            GroupBoxConnection.Enabled = false;
        }

        private void LoadingFinished()
        {
            PictureBoxLoading.Visible = false;
            ComboBoxParameter.Enabled = true;
            TextBoxParameterType.Enabled = true;
            TextBoxParameterValue.Enabled = true;
            ButtonSetValue.Enabled = true;
            GroupBoxConnection.Enabled = true;
        }
    }
}