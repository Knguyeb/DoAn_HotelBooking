using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DoAn_HotelBooking.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HangThanhVien",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenHang = table.Column<string>(type: "text", nullable: false),
                    MocDiemToiThieu = table.Column<int>(type: "integer", nullable: false),
                    TyLeGiamGia = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangThanhVien", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KhachSan",
                columns: table => new
                {
                    MaKhachSan = table.Column<string>(type: "text", nullable: false),
                    TenKhachSan = table.Column<string>(type: "text", nullable: false),
                    DiaChi = table.Column<string>(type: "text", nullable: false),
                    ViDo = table.Column<string>(type: "text", nullable: true),
                    KinhDo = table.Column<string>(type: "text", nullable: true),
                    SoDienThoai = table.Column<string>(type: "text", nullable: false),
                    HinhAnh = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachSan", x => x.MaKhachSan);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoiDung = table.Column<string>(type: "text", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaDoc = table.Column<bool>(type: "boolean", nullable: false),
                    MaKhachSan = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Phong",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoPhong = table.Column<int>(type: "integer", nullable: false),
                    Tang = table.Column<int>(type: "integer", nullable: false),
                    LoaiPhong = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SucChua = table.Column<int>(type: "integer", nullable: false),
                    GiaPhong = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HinhAnh = table.Column<string>(type: "text", nullable: false),
                    MaKhachSan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Phong_KhachSan_MaKhachSan",
                        column: x => x.MaKhachSan,
                        principalTable: "KhachSan",
                        principalColumn: "MaKhachSan");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HoVaTen = table.Column<string>(type: "text", nullable: false),
                    TenDangNhap = table.Column<string>(type: "text", nullable: false),
                    MatKhau = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SoDienThoai = table.Column<string>(type: "text", nullable: false),
                    QuyenHan = table.Column<string>(type: "text", nullable: false),
                    DiemTichLuy = table.Column<int>(type: "integer", nullable: false),
                    MaHang = table.Column<int>(type: "integer", nullable: true),
                    MaKhachSan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_HangThanhVien_MaHang",
                        column: x => x.MaHang,
                        principalTable: "HangThanhVien",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TaiKhoan_KhachSan_MaKhachSan",
                        column: x => x.MaKhachSan,
                        principalTable: "KhachSan",
                        principalColumn: "MaKhachSan");
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaKS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaKhachSan = table.Column<string>(type: "text", nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "integer", nullable: false),
                    SoSao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaKS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DanhGiaKS_KhachSan_MaKhachSan",
                        column: x => x.MaKhachSan,
                        principalTable: "KhachSan",
                        principalColumn: "MaKhachSan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaKS_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaPhong",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaPhong = table.Column<int>(type: "integer", nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "integer", nullable: false),
                    SoSao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaPhong", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DanhGiaPhong_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaPhong_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatPhong",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NgayTao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NgayNhanPhong = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NgayTraPhong = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SoNguoi = table.Column<int>(type: "integer", nullable: false),
                    TongTien = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TienGiam = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TrangThaiDatPhong = table.Column<string>(type: "text", nullable: false),
                    TrangThaiThanhToan = table.Column<string>(type: "text", nullable: false),
                    GhiChu = table.Column<string>(type: "text", nullable: true),
                    MaTaiKhoan = table.Column<int>(type: "integer", nullable: false),
                    MaPhong = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatPhong", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DatPhong_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DatPhong_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaKS_MaKhachSan",
                table: "DanhGiaKS",
                column: "MaKhachSan");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaKS_MaTaiKhoan",
                table: "DanhGiaKS",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaPhong_MaPhong",
                table: "DanhGiaPhong",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaPhong_MaTaiKhoan",
                table: "DanhGiaPhong",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_MaPhong",
                table: "DatPhong",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_DatPhong_MaTaiKhoan",
                table: "DatPhong",
                column: "MaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaKhachSan",
                table: "Phong",
                column: "MaKhachSan");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaHang",
                table: "TaiKhoan",
                column: "MaHang");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaKhachSan",
                table: "TaiKhoan",
                column: "MaKhachSan");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGiaKS");

            migrationBuilder.DropTable(
                name: "DanhGiaPhong");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "DatPhong");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "Phong");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "HangThanhVien");

            migrationBuilder.DropTable(
                name: "KhachSan");
        }
    }
}
