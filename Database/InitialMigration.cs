using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Zimmet_Bakim_Takip.Database
{
    // InitialMigration class
    // Not: Normalde bu sınıf 'dotnet ef migrations add InitialMigration' komutuyla otomatik oluşturulur.
    // Bu dosya sadece örnek olması için eklenmiştir.
    
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cihazlar tablosu
            migrationBuilder.CreateTable(
                name: "Cihazlar",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(nullable: false),
                    Tur = table.Column<string>(nullable: false),
                    Marka = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    SeriNo = table.Column<string>(nullable: true),
                    GarantiBaslangicTarihi = table.Column<DateTime>(nullable: true),
                    GarantiBitisTarihi = table.Column<DateTime>(nullable: true),
                    Aktif = table.Column<bool>(nullable: false, defaultValue: true),
                    Aciklama = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cihazlar", x => x.Id);
                });

            // Personeller tablosu
            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ad = table.Column<string>(nullable: false),
                    Soyad = table.Column<string>(nullable: false),
                    Departman = table.Column<string>(nullable: true),
                    Gorev = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Telefon = table.Column<string>(nullable: true),
                    Aktif = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                });

            // Zimmetler tablosu
            migrationBuilder.CreateTable(
                name: "Zimmetler",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonelId = table.Column<int>(nullable: false),
                    CihazId = table.Column<int>(nullable: false),
                    ZimmetTarihi = table.Column<DateTime>(nullable: false),
                    IadeTarihi = table.Column<DateTime>(nullable: true),
                    Aciklama = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zimmetler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Cihazlar_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihazlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Bakimlar tablosu
            migrationBuilder.CreateTable(
                name: "Bakimlar",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CihazId = table.Column<int>(nullable: false),
                    PlanlananTarih = table.Column<DateTime>(nullable: false),
                    GerceklesenTarih = table.Column<DateTime>(nullable: true),
                    BakimTuru = table.Column<string>(nullable: true),
                    YapilanIslem = table.Column<string>(nullable: true),
                    Aciklama = table.Column<string>(nullable: true),
                    Tamamlandi = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bakimlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bakimlar_Cihazlar_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihazlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // İndeksler
            migrationBuilder.CreateIndex(
                name: "IX_Bakimlar_CihazId",
                table: "Bakimlar",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_CihazId",
                table: "Zimmetler",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_PersonelId",
                table: "Zimmetler",
                column: "PersonelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Bakimlar");
            migrationBuilder.DropTable(name: "Zimmetler");
            migrationBuilder.DropTable(name: "Cihazlar");
            migrationBuilder.DropTable(name: "Personeller");
        }
    }
} 