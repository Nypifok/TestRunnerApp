package testrunner.app.viewmodel

import kotlinx.coroutines.*
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import testrunner.app.domain.entities.Test
import testrunner.app.model.TestSessionModel

class TestExplorerViewModel(private val model: TestSessionModel, private val viewModelScope: CoroutineScope) {

    private val _tests = MutableStateFlow<List<Test>>(emptyList())
    val tests: StateFlow<List<Test>> = _tests.asStateFlow()
    private var currentOperation: Job? = null

    init {
        viewModelScope.launch {
            model.tests.collect { updatedTests ->
                _tests.value = updatedTests
            }
        }
    }

    suspend fun runAllTests() {
        val job = viewModelScope.launch {
            withContext(Dispatchers.IO) {
                model.runTests(model.tests.value)
            }
        }
        currentOperation = job
        job.join()
    }

    suspend fun runSelectedTests(selectedTests: List<Test>) {
        val job = viewModelScope.launch {
            withContext(Dispatchers.IO) {
                model.runTests(selectedTests)
            }
        }
        currentOperation = job
        job.join()
    }

    suspend fun discoverTests(paths: List<String>) {
        val job = viewModelScope.launch {
            withContext(Dispatchers.IO) {
                model.discoverTests(paths)
            }
        }
        currentOperation = job
        job.join()
    }

    suspend fun cancelOperation() {
        val job = viewModelScope.launch {
            withContext(Dispatchers.IO) {
                model.cancelCurrentOperation()
            }
            //Waits for last notifications
            delay(100)
            currentOperation?.cancel()
        }
        job.join()
    }
}