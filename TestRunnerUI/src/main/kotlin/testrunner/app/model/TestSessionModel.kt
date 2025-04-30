package testrunner.app.model

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import testrunner.app.domain.entities.Outcome
import testrunner.app.domain.entities.Test
import testrunner.app.model.contract.IStorageClient
import java.lang.Thread.sleep

class TestSessionModel(val storageClient: IStorageClient) {

    private val _tests = MutableStateFlow<List<Test>>(emptyList())
    val tests: StateFlow<List<Test>> get() = _tests

    init {

    }

    suspend fun discoverTests(paths: List<String>) {
        _tests.value = emptyList()
        storageClient.getTestCases(paths).collect { newTestCases ->
            updateTests(newTestCases)
        }
    }

    suspend fun runTests(testCases: List<Test> = tests.value) {
        val resetTests = testCases.map { it.copy(outcome = Outcome.None) }
        updateTests(resetTests)
        storageClient.runSelectedTests(resetTests).collect { newTestCases ->
            updateTests(newTestCases)
        }
    }

    suspend fun cancelCurrentOperation() {
        storageClient.cancelCurrentOperation()
    }

    private fun updateTests(updates: List<Test>) {

        val updatesById = updates.associateBy { it.id }

        val updatedTests = tests.value.map { test ->
            updatesById[test.id] ?: test
        }

        val existingIds = tests.value.mapTo(mutableSetOf()) { it.id }
        val newTests = updates.filter { it.id !in existingIds }

        _tests.value = updatedTests + newTests
    }
}