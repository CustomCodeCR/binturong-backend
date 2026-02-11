namespace Application.Security.Scopes;

public static partial class SecurityScopes
{
    public const string ContractsRead = "contracts.read";
    public const string ContractsCreate = "contracts.create";
    public const string ContractsUpdate = "contracts.update";
    public const string ContractsDelete = "contracts.delete";

    public const string ContractsMilestonesManage = "contracts.milestones.manage";

    public const string ContractsAttachmentsUpload = "contracts.attachments.upload";
    public const string ContractsAttachmentsDelete = "contracts.attachments.delete";
}
