package testrunner.app.domain.entities

enum class Outcome {
    None,
    Passed,
    Failed,
    Skipped,
    NotFound,
}

data class Test(
    var id: String,
    var displayName: String,
    var fullyQualifiedName: String,
    var executorUri: String,
    var source: String,
    var durationMs: Int,
    var outcome: Outcome,
    var errorMessage: String,
    var errorStackTrace: String
)