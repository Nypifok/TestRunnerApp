package testrunner.app.model.contract

import kotlinx.coroutines.flow.Flow
import testrunner.app.domain.entities.Test

interface IStorageClient {
    suspend fun getTestCases(paths: List<String>): Flow<List<Test>>
    suspend fun runSelectedTests(testCases: List<Test>): Flow<List<Test>>
    suspend fun cancelCurrentOperation()
}