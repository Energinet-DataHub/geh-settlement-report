# The 'views.dsl' file is intended as a mean for viewing and validating the model
# in the domain repository. It should
#   * Extend the base model and override the 'dh3' software system
#   * Include of the `model.dsl` files from each domain repository using an URL
#
# The `model.dsl` file must contain the actual model, and is the piece that must
# be reusable and included in other Structurizr files like `views.dsl` and
# deployment diagram files.

workspace extends https://raw.githubusercontent.com/Energinet-DataHub/opengeh-arch-diagrams/main/docs/diagrams/c4-model/dh-base-model.dsl {

    model {
        #
        # DataHub 3.0 (extends)
        #
        !ref dh3 {

            # Include model.
            !include model.dsl

            # IMPORTANT:
            # The order by which models are included is important for how the domain-to-domain relationships are specified.
            # A domain-to-domain relationship should be specified in the "client" of a "client->server" dependency, and
            # hence domains that doesn't depend on others, should be listed first.

            relatedSubsystems = group "Related Subsystems" {

                bffSubsystem = container "BFF" {
                    description "Backend for Frontend"
                    tags "Subsystem"

                    this -> settlementReportOrchestrator "uses HTTP API"
                    this -> settlementReportApi "uses HTTP API"
                }

            }

            futureComponent = group "Job API component" {

                jobApi = container "Job API" {
                    description "Backend for Frontend"
                    tags "Subsystem"

                    settlementReportWorker -> this "uses Job API"
                    settlementReportApi -> this "uses Job API"
                    this -> settlementReportBlobStorage "writes reports"
                }

            }

        }
    }

    views {
        container dh3 "SettlementReport" {
            title "[Container] DataHub 3.0 - Settlement Reports"
            include ->settlementReportDomain->
            exclude "element.tag==Intermediate Technology"
            exclude "relationship.tag==Detailed View"
        }

        container dh3 "SettlementReportDetailed" {
            title "[Container] DataHub 3.0 - Settlement Reports (Detailed)"
            include ->settlementReportDomain->
            exclude "relationship.tag==Simple View"
        }
    }
}
