using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeloCarbon.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emission_factors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subcategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_factors", x => x.Id);
                    table.CheckConstraint("ck_factor_nonneg", "\"Value\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPreference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scenarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaselineStart = table.Column<DateTime>(type: "date", nullable: false),
                    BaselineEnd = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scenarios_user_profiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FuelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_user_profiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenario_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetActivityRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetApplianceUsageId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scenario_changes_scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenario_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaselineKgCO2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ScenarioKgCO2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ReductionKgCO2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scenario_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scenario_results_scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalTable: "scenarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Unit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    InputMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    UsageKWh = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "date", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "date", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    FuelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LitersPurchased = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TotalCost = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    MobilityActivity_VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivityDate = table.Column<DateTime>(type: "date", nullable: true),
                    TransportMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DistanceKm = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Frequency = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_records", x => x.Id);
                    table.CheckConstraint("ck_mobility_positive", "\"ActivityType\" <> 'Mobility' OR (\"DistanceKm\" > 0 AND \"Frequency\" >= 1)");
                    table.ForeignKey(
                        name: "FK_activity_records_user_profiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activity_records_vehicles_MobilityActivity_VehicleId",
                        column: x => x.MobilityActivity_VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_records_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_monthly_inputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<DateTime>(type: "date", nullable: false),
                    Method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_monthly_inputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_monthly_inputs_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScenarioResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetCategory = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecommendedAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EstimatedReductionKgCO2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recommendations_scenario_results_ScenarioResultId",
                        column: x => x.ScenarioResultId,
                        principalTable: "scenario_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appliance_usages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElectricityUsageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PowerWatt = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    HoursPerDay = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DaysUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appliance_usages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appliance_usages_activity_records_ElectricityUsageId",
                        column: x => x.ElectricityUsageId,
                        principalTable: "activity_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "emission_calculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmissionFactorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ResultKgCO2e = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emission_calculations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_emission_calculations_activity_records_ActivityRecordId",
                        column: x => x.ActivityRecordId,
                        principalTable: "activity_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_emission_calculations_emission_factors_EmissionFactorId",
                        column: x => x.EmissionFactorId,
                        principalTable: "emission_factors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_records_MobilityActivity_VehicleId",
                table: "activity_records",
                column: "MobilityActivity_VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_activity_records_UserProfileId",
                table: "activity_records",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_activity_records_UserProfileId_PeriodStart",
                table: "activity_records",
                columns: new[] { "UserProfileId", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activity_records_VehicleId",
                table: "activity_records",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_appliance_usages_ElectricityUsageId",
                table: "appliance_usages",
                column: "ElectricityUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_emission_calculations_ActivityRecordId",
                table: "emission_calculations",
                column: "ActivityRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emission_calculations_EmissionFactorId",
                table: "emission_calculations",
                column: "EmissionFactorId");

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_ScenarioResultId",
                table: "recommendations",
                column: "ScenarioResultId");

            migrationBuilder.CreateIndex(
                name: "IX_scenario_changes_ScenarioId",
                table: "scenario_changes",
                column: "ScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_scenario_results_ScenarioId",
                table: "scenario_results",
                column: "ScenarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scenarios_UserProfileId",
                table: "scenarios",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_monthly_inputs_VehicleId_Month",
                table: "vehicle_monthly_inputs",
                columns: new[] { "VehicleId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_UserProfileId",
                table: "vehicles",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appliance_usages");

            migrationBuilder.DropTable(
                name: "emission_calculations");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "scenario_changes");

            migrationBuilder.DropTable(
                name: "vehicle_monthly_inputs");

            migrationBuilder.DropTable(
                name: "activity_records");

            migrationBuilder.DropTable(
                name: "emission_factors");

            migrationBuilder.DropTable(
                name: "scenario_results");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "scenarios");

            migrationBuilder.DropTable(
                name: "user_profiles");
        }
    }
}
