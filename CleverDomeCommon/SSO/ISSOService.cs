using CleverDomeCommon.SSO;



[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace = "urn:up-us:sso-service:service:v1", ConfigurationName = "ISSOService")]
public interface ISSOService
{

    // CODEGEN: Generating message contract since the operation GetSSO is neither RPC nor document wrapped.
    [System.ServiceModel.OperationContractAttribute(Action = "urn:up-us:sso-service:service:v1/ISSOService/GetSSO", ReplyAction = "urn:up-us:sso-service:service:v1/ISSOService/GetSSOResponse")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(StatusResponseType))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(SignaturePropertiesType))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(ManifestType))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(customFault))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AgreementMethodType))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(EncryptedType))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(RequestAbstractType))]
    GetSSOResponse GetSSO(GetSSORequest request);
}



/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "urn:up-us:sso-service:messages:v1")]
public partial class customFault
{

    private string errorCodeField;

    private string messageField;

    private string[] messagesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
    public string errorCode
    {
        get
        {
            return this.errorCodeField;
        }
        set
        {
            this.errorCodeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
    public string message
    {
        get
        {
            return this.messageField;
        }
        set
        {
            this.messageField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("messages", Order = 2)]
    public string[] messages
    {
        get
        {
            return this.messagesField;
        }
        set
        {
            this.messagesField = value;
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
public partial class GetSSOResponse
{

    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", Order = 0)]
    public ResponseType Response;

    public GetSSOResponse()
    {
    }

    public GetSSOResponse(ResponseType Response)
    {
        this.Response = Response;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
public partial class GetSSORequest
{

    [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "urn:oasis:names:tc:SAML:2.0:protocol", Order = 0)]
    public AuthnRequestType AuthnRequest;

    public GetSSORequest()
    {
    }

    public GetSSORequest(AuthnRequestType AuthnRequest)
    {
        this.AuthnRequest = AuthnRequest;
    }
}