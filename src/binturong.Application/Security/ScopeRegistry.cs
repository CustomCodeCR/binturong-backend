namespace Application.Security;

public static class ScopeRegistry
{
    public static readonly IReadOnlyList<ScopeDefinition> All = new[]
    {
        // =========================
        // Users
        // =========================
        new ScopeDefinition("users.read", "View users", Roles.Admin),
        new ScopeDefinition("users.create", "Create users", Roles.Admin),
        new ScopeDefinition("users.update", "Update users", Roles.Admin),
        new ScopeDefinition("users.delete", "Delete users", Roles.Admin),
        new ScopeDefinition("users.roles.assign", "Assign/remove roles to users", Roles.Admin),
        // =========================
        // Roles
        // =========================
        new ScopeDefinition("roles.read", "View roles", Roles.Admin),
        new ScopeDefinition("roles.create", "Create roles", Roles.Admin),
        new ScopeDefinition("roles.update", "Update roles", Roles.Admin),
        new ScopeDefinition("roles.delete", "Delete roles", Roles.Admin),
        new ScopeDefinition("roles.scopes.assign", "Assign/remove scopes to roles", Roles.Admin),
        // =========================
        // Branches
        // =========================
        new ScopeDefinition("branches.read", "View branches", Roles.Admin, Roles.Manager),
        new ScopeDefinition("branches.create", "Create branches", Roles.Admin),
        new ScopeDefinition("branches.update", "Update branches", Roles.Admin),
        new ScopeDefinition("branches.delete", "Delete branches", Roles.Admin),
        // =========================
        // Warehouses
        // =========================
        new ScopeDefinition("warehouses.read", "View warehouses", Roles.Admin, Roles.Manager),
        new ScopeDefinition("warehouses.create", "Create warehouses", Roles.Admin),
        new ScopeDefinition("warehouses.update", "Update warehouses", Roles.Admin),
        new ScopeDefinition("warehouses.delete", "Delete warehouses", Roles.Admin),
        // =========================
        // Products
        // =========================
        new ScopeDefinition(
            "products.read",
            "View products",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition("products.create", "Create products", Roles.Admin),
        new ScopeDefinition("products.update", "Update products", Roles.Admin),
        new ScopeDefinition("products.delete", "Delete products", Roles.Admin),
        // =========================
        // Categories
        // =========================
        new ScopeDefinition(
            "categories.read",
            "View product categories",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition("categories.create", "Create product categories", Roles.Admin),
        new ScopeDefinition("categories.update", "Update product categories", Roles.Admin),
        new ScopeDefinition("categories.delete", "Delete product categories", Roles.Admin),
        // =========================
        // Taxes
        // =========================
        new ScopeDefinition("taxes.read", "View taxes", Roles.Admin, Roles.Manager),
        new ScopeDefinition("taxes.create", "Create taxes", Roles.Admin),
        new ScopeDefinition("taxes.update", "Update taxes", Roles.Admin),
        new ScopeDefinition("taxes.delete", "Delete taxes", Roles.Admin),
        // =========================
        // Units of Measure (UOMs)
        // =========================
        new ScopeDefinition("uoms.read", "View units of measure", Roles.Admin, Roles.Manager),
        new ScopeDefinition("uoms.create", "Create units of measure", Roles.Admin),
        new ScopeDefinition("uoms.update", "Update units of measure", Roles.Admin),
        new ScopeDefinition("uoms.delete", "Delete units of measure", Roles.Admin),
        // =========================
        // Inventory
        // =========================
        new ScopeDefinition(
            "inventory.movements.create",
            "Register inventory movement",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition(
            "inventory.transfers.read",
            "View inventory transfers",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition(
            "inventory.transfers.create",
            "Create inventory transfer",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition("inventory.transfers.update", "Update inventory transfer", Roles.Admin),
        new ScopeDefinition("inventory.transfers.delete", "Delete inventory transfer", Roles.Admin),
        new ScopeDefinition(
            "inventory.transfers.request_review",
            "Request review for inventory transfer",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "inventory.transfers.approve",
            "Approve inventory transfer",
            Roles.Admin
        ),
        new ScopeDefinition("inventory.transfers.reject", "Reject inventory transfer", Roles.Admin),
        new ScopeDefinition(
            "inventory.transfers.confirm",
            "Confirm inventory transfer",
            Roles.Admin
        ),
        new ScopeDefinition("inventory.transfers.cancel", "Cancel inventory transfer", Roles.Admin),
        new ScopeDefinition(
            "inventory.by_branch.read",
            "View inventory by branch",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition(
            "inventory.stock.read",
            "View consolidated stock",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Suppliers
        // =========================
        new ScopeDefinition("suppliers.read", "View suppliers", Roles.Admin, Roles.Manager),
        new ScopeDefinition("suppliers.create", "Create suppliers", Roles.Admin),
        new ScopeDefinition("suppliers.update", "Update suppliers", Roles.Admin),
        new ScopeDefinition("suppliers.delete", "Delete suppliers", Roles.Admin),
        new ScopeDefinition(
            "suppliers.credit.assign",
            "Assign/update supplier credit conditions",
            Roles.Admin
        ),
        // =========================
        // Clients
        // =========================
        new ScopeDefinition("clients.read", "View clients", Roles.Admin, Roles.Manager),
        new ScopeDefinition("clients.create", "Create clients", Roles.Admin),
        new ScopeDefinition("clients.update", "Update clients", Roles.Admin),
        new ScopeDefinition("clients.delete", "Delete clients", Roles.Admin),
        // =========================
        // Employees
        // =========================
        new ScopeDefinition("employees.read", "View employees", Roles.Admin, Roles.Manager),
        new ScopeDefinition("employees.create", "Create employees", Roles.Admin),
        new ScopeDefinition("employees.update", "Update employees", Roles.Admin),
        new ScopeDefinition("employees.delete", "Delete employees", Roles.Admin),
        new ScopeDefinition(
            "employees.attendance.checkin",
            "Employee check-in",
            Roles.Admin,
            Roles.Operator
        ),
        new ScopeDefinition(
            "employees.attendance.checkout",
            "Employee check-out",
            Roles.Admin,
            Roles.Operator
        ),
        // =========================
        // Security / System
        // =========================
        new ScopeDefinition("security.scopes.read", "List scopes", Roles.Admin),
        new ScopeDefinition("security.admin.reset_password", "Reset admin password", Roles.Admin),
        // System-protected role control
        new ScopeDefinition("roles.system.protect", "Protect system roles", Roles.SuperAdmin),
        // =========================
        // Auth (usually public)
        // =========================
        new ScopeDefinition("auth.login", "Login"),
        // =========================
        // Audit
        // =========================
        new ScopeDefinition("audit.read", "View audit logs", Roles.Admin),
        new ScopeDefinition("audit.write", "Register audit actions", Roles.Admin, Roles.Manager),
        new ScopeDefinition("audit.export", "Export audit logs", Roles.Admin),
        // =========================
        // Quotes (Sales)
        // =========================
        new ScopeDefinition(
            "quotes.read",
            "View quotes",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition("quotes.create", "Create quotes", Roles.Admin, Roles.Manager),
        new ScopeDefinition("quotes.update", "Update quotes", Roles.Admin, Roles.Manager),
        new ScopeDefinition("quotes.delete", "Delete quotes", Roles.Admin),
        // Lifecycle actions
        new ScopeDefinition("quotes.send", "Send quote", Roles.Admin, Roles.Manager),
        new ScopeDefinition("quotes.accept", "Accept quote", Roles.Admin, Roles.Manager),
        new ScopeDefinition("quotes.reject", "Reject quote", Roles.Admin, Roles.Manager),
        new ScopeDefinition("quotes.expire", "Expire quote", Roles.Admin, Roles.Manager),
        // Details (lines)
        new ScopeDefinition("quotes.details.add", "Add quote line", Roles.Admin, Roles.Manager),
        new ScopeDefinition(
            "quotes.details.update",
            "Update quote line",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "quotes.details.delete",
            "Delete quote line",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Accounts Payable
        // =========================
        new ScopeDefinition(
            "accounts_payable.read",
            "View accounts payable",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "accounts_payable.payments.create",
            "Register accounts payable payment",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Supplier Quotes
        // =========================
        new ScopeDefinition(
            "supplier_quotes.read",
            "View supplier quotes",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "supplier_quotes.create",
            "Create supplier quote request",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "supplier_quotes.respond",
            "Register supplier quote response",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "supplier_quotes.reject",
            "Reject supplier quote",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Supplier Evaluations
        // =========================
        new ScopeDefinition(
            "supplier_evaluations.read",
            "View supplier evaluations",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "supplier_evaluations.create",
            "Create supplier evaluation",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Purchases - Purchase Requests
        // =========================
        new ScopeDefinition(
            "purchase_requests.read",
            "View purchase requests",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "purchase_requests.create",
            "Create purchase requests",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Purchases - Purchase Orders
        // =========================
        new ScopeDefinition(
            "purchase_orders.read",
            "View purchase orders",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "purchase_orders.create",
            "Create purchase orders",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Purchases - Purchase Receipts
        // =========================
        new ScopeDefinition(
            "purchase_receipts.read",
            "View purchase receipts",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "purchase_receipts.create",
            "Create purchase receipts",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "purchase_receipts.reject",
            "Reject purchase receipts",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Sales Orders (Sales)
        // =========================
        new ScopeDefinition(
            "sales_orders.read",
            "View sales orders",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition(
            "sales_orders.create",
            "Create sales orders",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "sales_orders.convert_from_quote",
            "Convert quote to sales order",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "sales_orders.confirm",
            "Confirm sales order",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Contracts
        // =========================
        new ScopeDefinition(
            "contracts.read",
            "View contracts",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition("contracts.create", "Create contracts", Roles.Admin, Roles.Manager),
        new ScopeDefinition("contracts.update", "Update contracts", Roles.Admin, Roles.Manager),
        new ScopeDefinition("contracts.delete", "Delete contracts", Roles.Admin),
        new ScopeDefinition(
            "contracts.milestones.manage",
            "Manage contract billing milestones",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "contracts.attachments.upload",
            "Upload contract attachments",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "contracts.attachments.delete",
            "Delete contract attachments",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "contracts.convert_from_quote",
            "Convert accepted quote to contract",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Payroll
        // =========================
        new ScopeDefinition("payroll.read", "View payroll", Roles.Admin, Roles.Manager),
        new ScopeDefinition(
            "payroll.create",
            "Create/calculate payroll",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition("payroll.update", "Update payroll", Roles.Admin, Roles.Manager),
        new ScopeDefinition(
            "payroll.overtime.manage",
            "Manage overtime entries",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "payroll.commission.manage",
            "Adjust commissions",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "payroll.payslip.read",
            "Download payslips",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "payroll.payslip.send",
            "Send payslips by email",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "payroll.export",
            "Export payroll/payment history",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Invoices
        // =========================
        new ScopeDefinition(
            "invoices.read",
            "View invoices",
            Roles.Admin,
            Roles.Manager,
            Roles.Operator
        ),
        new ScopeDefinition("invoices.create", "Create invoices", Roles.Admin, Roles.Manager),
        new ScopeDefinition("invoices.update", "Update invoices", Roles.Admin, Roles.Manager),
        new ScopeDefinition("invoices.delete", "Delete invoices", Roles.Admin),
        new ScopeDefinition("invoices.emit", "Emit electronic invoice", Roles.Admin, Roles.Manager),
        // =========================
        // Payments (Accounts Receivable)
        // =========================
        new ScopeDefinition("payments.read", "View payments", Roles.Admin, Roles.Manager),
        new ScopeDefinition("payments.create", "Register payments", Roles.Admin, Roles.Manager),
        new ScopeDefinition("payments.delete", "Delete payments", Roles.Admin),
        new ScopeDefinition("payments.register", "Register payments", Roles.Admin, Roles.Manager),
        new ScopeDefinition("payments.export", "Export payments", Roles.Admin),
        // =========================
        // Credit Notes
        // =========================
        new ScopeDefinition("credit_notes.read", "View credit notes", Roles.Admin, Roles.Manager),
        new ScopeDefinition(
            "credit_notes.create",
            "Create credit notes",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition("credit_notes.delete", "Delete credit notes", Roles.Admin),
        new ScopeDefinition("credit_notes.emit", "Emit credit notes", Roles.Admin, Roles.Manager),
        // =========================
        // Debit Notes
        // =========================
        new ScopeDefinition("debit_notes.read", "View debit notes", Roles.Admin, Roles.Manager),
        new ScopeDefinition("debit_notes.create", "Create debit notes", Roles.Admin, Roles.Manager),
        new ScopeDefinition("debit_notes.delete", "Delete debit notes", Roles.Admin),
        new ScopeDefinition("debit_notes.emit", "Emit debit notes", Roles.Admin, Roles.Manager),
        new ScopeDefinition(
            "invoices.convert_from_quote",
            "Convert accepted quote to invoice",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition(
            "accounts_receivable.read",
            "View accounts receivable status",
            Roles.Admin,
            Roles.Manager
        ),
        // =========================
        // Payments Methods
        // =========================
        new ScopeDefinition(
            "payment_methods.read",
            "View payment methods",
            Roles.Admin,
            Roles.Manager
        ),
        new ScopeDefinition("payment_methods.create", "Create payment methods", Roles.Admin),
        new ScopeDefinition("payment_methods.update", "Update payment methods", Roles.Admin),
        new ScopeDefinition("payment_methods.delete", "Delete payment methods", Roles.Admin),
    };

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Operator = "Operator";
    }
}
