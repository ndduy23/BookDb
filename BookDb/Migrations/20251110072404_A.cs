using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookDb.Migrations
{
    /// <inheritdoc />
    public partial class A : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_DocumentPages_DocumentPageId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_DocumentPageId",
                table: "Bookmarks");

            // 1. Make Author column NULLABLE
            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "Documents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            // 2. Add new columns to Documents
            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Documents",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            // 3. Update Bookmarks table
            migrationBuilder.AlterColumn<int>(
                name: "DocumentPageId",
                table: "Bookmarks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Bookmarks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                table: "Bookmarks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Bookmarks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            // 4. Create Authors table
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                    table.UniqueConstraint("AK_Authors_Name", x => x.Name);
                });

            // 5. Migrate existing author data to Authors table
            migrationBuilder.Sql(@"
                INSERT INTO Authors (Name)
                SELECT DISTINCT Author
                FROM Documents
                WHERE Author IS NOT NULL AND Author <> ''
                AND NOT EXISTS (SELECT 1 FROM Authors WHERE Name = Documents.Author)
            ");

            // 6. Set AuthorId for existing documents
            migrationBuilder.Sql(@"
                UPDATE d
                SET d.AuthorId = a.Id
                FROM Documents d
                INNER JOIN Authors a ON d.Author = a.Name
                WHERE d.Author IS NOT NULL
            ");

            // 7. Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Documents_Author",
                table: "Documents",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AuthorId",
                table: "Documents",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_DocumentId_PageNumber",
                table: "Bookmarks",
                columns: new[] { "DocumentId", "PageNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_DocumentPageId",
                table: "Bookmarks",
                column: "DocumentPageId",
                unique: true,
                filter: "[DocumentPageId] IS NOT NULL");

            // 8. Add foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_DocumentPages_DocumentPageId",
                table: "Bookmarks",
                column: "DocumentPageId",
                principalTable: "DocumentPages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_Documents_DocumentId",
                table: "Bookmarks",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 9. Add FK from Documents.Author to Authors.Name (now all authors exist)
            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Authors_Author",
                table: "Documents",
                column: "Author",
                principalTable: "Authors",
                principalColumn: "Name",
                onDelete: ReferentialAction.SetNull);

            // 10. Add FK from Documents.AuthorId to Authors.Id
            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Authors_AuthorId",
                table: "Documents",
                column: "AuthorId",
                principalTable: "Authors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_DocumentPages_DocumentPageId",
                table: "Bookmarks");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookmarks_Documents_DocumentId",
                table: "Bookmarks");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Authors_Author",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Authors_AuthorId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Author",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_AuthorId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_DocumentId_PageNumber",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_DocumentPageId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "PageNumber",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bookmarks");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentPageId",
                table: "Bookmarks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "Documents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_DocumentPageId",
                table: "Bookmarks",
                column: "DocumentPageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmarks_DocumentPages_DocumentPageId",
                table: "Bookmarks",
                column: "DocumentPageId",
                principalTable: "DocumentPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}