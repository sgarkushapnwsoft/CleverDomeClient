﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PortalManagement
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PortalUser.UserEmail", Namespace="http://schemas.datacontract.org/2004/07/PortalManagement")]
    public partial class PortalUserUserEmail : PortalManagement.EmailAddress
    {
        
        private bool IsPrimaryField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsPrimary
        {
            get
            {
                return this.IsPrimaryField;
            }
            set
            {
                this.IsPrimaryField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="EmailAddress", Namespace="http://schemas.datacontract.org/2004/07/PortalManagement")]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(PortalManagement.PortalUserUserEmail))]
    public partial class EmailAddress : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private bool ActiveField;
        
        private string EmailField;
        
        private int IDField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Active
        {
            get
            {
                return this.ActiveField;
            }
            set
            {
                this.ActiveField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email
        {
            get
            {
                return this.EmailField;
            }
            set
            {
                this.EmailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ID
        {
            get
            {
                return this.IDField;
            }
            set
            {
                this.IDField = value;
            }
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="IVendorManagement")]
public interface IVendorManagement
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/CreateUser", ReplyAction="http://tempuri.org/IVendorManagement/CreateUserResponse")]
    int CreateUser(string externalUserID, string vendorName, string firstName, string lastName, string email, string phone);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/CreateUser", ReplyAction="http://tempuri.org/IVendorManagement/CreateUserResponse")]
    System.Threading.Tasks.Task<int> CreateUserAsync(string externalUserID, string vendorName, string firstName, string lastName, string email, string phone);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/CheckUser", ReplyAction="http://tempuri.org/IVendorManagement/CheckUserResponse")]
    System.Nullable<int> CheckUser(string externaUserID, string vendorName);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/CheckUser", ReplyAction="http://tempuri.org/IVendorManagement/CheckUserResponse")]
    System.Threading.Tasks.Task<System.Nullable<int>> CheckUserAsync(string externaUserID, string vendorName);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/AllowAccessToUserTagsHierarchy", ReplyAction="http://tempuri.org/IVendorManagement/AllowAccessToUserTagsHierarchyResponse")]
    void AllowAccessToUserTagsHierarchy(string vendorName, int accessorID, int sharedUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/AllowAccessToUserTagsHierarchy", ReplyAction="http://tempuri.org/IVendorManagement/AllowAccessToUserTagsHierarchyResponse")]
    System.Threading.Tasks.Task AllowAccessToUserTagsHierarchyAsync(string vendorName, int accessorID, int sharedUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/DenyAccessToUserTagsHierarchy", ReplyAction="http://tempuri.org/IVendorManagement/DenyAccessToUserTagsHierarchyResponse")]
    void DenyAccessToUserTagsHierarchy(string vendorName, int accessorID, int sharedUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/DenyAccessToUserTagsHierarchy", ReplyAction="http://tempuri.org/IVendorManagement/DenyAccessToUserTagsHierarchyResponse")]
    System.Threading.Tasks.Task DenyAccessToUserTagsHierarchyAsync(string vendorName, int accessorID, int sharedUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/DeleteUser", ReplyAction="http://tempuri.org/IVendorManagement/DeleteUserResponse")]
    void DeleteUser(string externalUserID, string vendorName);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/DeleteUser", ReplyAction="http://tempuri.org/IVendorManagement/DeleteUserResponse")]
    System.Threading.Tasks.Task DeleteUserAsync(string externalUserID, string vendorName);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/GetUserEmails", ReplyAction="http://tempuri.org/IVendorManagement/GetUserEmailsResponse")]
    PortalManagement.PortalUserUserEmail[] GetUserEmails(string vendorName, string externalUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/GetUserEmails", ReplyAction="http://tempuri.org/IVendorManagement/GetUserEmailsResponse")]
    System.Threading.Tasks.Task<PortalManagement.PortalUserUserEmail[]> GetUserEmailsAsync(string vendorName, string externalUserID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/AddUserEmail", ReplyAction="http://tempuri.org/IVendorManagement/AddUserEmailResponse")]
    int AddUserEmail(string vendorName, string externalUserID, string email, bool isPrimary);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/AddUserEmail", ReplyAction="http://tempuri.org/IVendorManagement/AddUserEmailResponse")]
    System.Threading.Tasks.Task<int> AddUserEmailAsync(string vendorName, string externalUserID, string email, bool isPrimary);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/SetUserPrimaryEmail", ReplyAction="http://tempuri.org/IVendorManagement/SetUserPrimaryEmailResponse")]
    void SetUserPrimaryEmail(string vendorName, string externalUserID, int emailID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/SetUserPrimaryEmail", ReplyAction="http://tempuri.org/IVendorManagement/SetUserPrimaryEmailResponse")]
    System.Threading.Tasks.Task SetUserPrimaryEmailAsync(string vendorName, string externalUserID, int emailID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/RemoveUserEmail", ReplyAction="http://tempuri.org/IVendorManagement/RemoveUserEmailResponse")]
    void RemoveUserEmail(string vendorName, string externalUserID, int emailID);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IVendorManagement/RemoveUserEmail", ReplyAction="http://tempuri.org/IVendorManagement/RemoveUserEmailResponse")]
    System.Threading.Tasks.Task RemoveUserEmailAsync(string vendorName, string externalUserID, int emailID);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IVendorManagementChannel : IVendorManagement, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class VendorManagementClient : System.ServiceModel.ClientBase<IVendorManagement>, IVendorManagement
{
    
    public VendorManagementClient()
    {
    }
    
    public VendorManagementClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public VendorManagementClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public VendorManagementClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public VendorManagementClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public int CreateUser(string externalUserID, string vendorName, string firstName, string lastName, string email, string phone)
    {
        return base.Channel.CreateUser(externalUserID, vendorName, firstName, lastName, email, phone);
    }
    
    public System.Threading.Tasks.Task<int> CreateUserAsync(string externalUserID, string vendorName, string firstName, string lastName, string email, string phone)
    {
        return base.Channel.CreateUserAsync(externalUserID, vendorName, firstName, lastName, email, phone);
    }
    
    public System.Nullable<int> CheckUser(string externaUserID, string vendorName)
    {
        return base.Channel.CheckUser(externaUserID, vendorName);
    }
    
    public System.Threading.Tasks.Task<System.Nullable<int>> CheckUserAsync(string externaUserID, string vendorName)
    {
        return base.Channel.CheckUserAsync(externaUserID, vendorName);
    }
    
    public void AllowAccessToUserTagsHierarchy(string vendorName, int accessorID, int sharedUserID)
    {
        base.Channel.AllowAccessToUserTagsHierarchy(vendorName, accessorID, sharedUserID);
    }
    
    public System.Threading.Tasks.Task AllowAccessToUserTagsHierarchyAsync(string vendorName, int accessorID, int sharedUserID)
    {
        return base.Channel.AllowAccessToUserTagsHierarchyAsync(vendorName, accessorID, sharedUserID);
    }
    
    public void DenyAccessToUserTagsHierarchy(string vendorName, int accessorID, int sharedUserID)
    {
        base.Channel.DenyAccessToUserTagsHierarchy(vendorName, accessorID, sharedUserID);
    }
    
    public System.Threading.Tasks.Task DenyAccessToUserTagsHierarchyAsync(string vendorName, int accessorID, int sharedUserID)
    {
        return base.Channel.DenyAccessToUserTagsHierarchyAsync(vendorName, accessorID, sharedUserID);
    }
    
    public void DeleteUser(string externalUserID, string vendorName)
    {
        base.Channel.DeleteUser(externalUserID, vendorName);
    }
    
    public System.Threading.Tasks.Task DeleteUserAsync(string externalUserID, string vendorName)
    {
        return base.Channel.DeleteUserAsync(externalUserID, vendorName);
    }
    
    public PortalManagement.PortalUserUserEmail[] GetUserEmails(string vendorName, string externalUserID)
    {
        return base.Channel.GetUserEmails(vendorName, externalUserID);
    }
    
    public System.Threading.Tasks.Task<PortalManagement.PortalUserUserEmail[]> GetUserEmailsAsync(string vendorName, string externalUserID)
    {
        return base.Channel.GetUserEmailsAsync(vendorName, externalUserID);
    }
    
    public int AddUserEmail(string vendorName, string externalUserID, string email, bool isPrimary)
    {
        return base.Channel.AddUserEmail(vendorName, externalUserID, email, isPrimary);
    }
    
    public System.Threading.Tasks.Task<int> AddUserEmailAsync(string vendorName, string externalUserID, string email, bool isPrimary)
    {
        return base.Channel.AddUserEmailAsync(vendorName, externalUserID, email, isPrimary);
    }
    
    public void SetUserPrimaryEmail(string vendorName, string externalUserID, int emailID)
    {
        base.Channel.SetUserPrimaryEmail(vendorName, externalUserID, emailID);
    }
    
    public System.Threading.Tasks.Task SetUserPrimaryEmailAsync(string vendorName, string externalUserID, int emailID)
    {
        return base.Channel.SetUserPrimaryEmailAsync(vendorName, externalUserID, emailID);
    }
    
    public void RemoveUserEmail(string vendorName, string externalUserID, int emailID)
    {
        base.Channel.RemoveUserEmail(vendorName, externalUserID, emailID);
    }
    
    public System.Threading.Tasks.Task RemoveUserEmailAsync(string vendorName, string externalUserID, int emailID)
    {
        return base.Channel.RemoveUserEmailAsync(vendorName, externalUserID, emailID);
    }
}
