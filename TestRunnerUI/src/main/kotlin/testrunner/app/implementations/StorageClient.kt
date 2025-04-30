package testrunner.app.implementations

import io.grpc.ManagedChannel
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.*
import processing_notification_service.ProcessingNotificationServiceGrpcKt
import processing_notification_service.ProcessingNotificationServiceOuterClass.Notification
import processing_notification_service.ProcessingNotificationServiceOuterClass.SubscribeRequest
import processing_notification_service.TestDiscovery.TestCase
import processing_notification_service.TestRun
import processing_notification_service.TestRun.TestResult
import processing_notification_service.TestSessionServiceGrpcKt
import processing_notification_service.TestSessionServiceOuterClass.CancelCurrentOperationRequest
import processing_notification_service.TestSessionServiceOuterClass.DiscoverTestsRequest
import processing_notification_service.TestSessionServiceOuterClass.RunSelectedTestsRequest
import testrunner.app.domain.entities.Outcome
import testrunner.app.domain.entities.Test
import testrunner.app.model.contract.IStorageClient
import java.util.*

class StorageClient(private val channel: ManagedChannel) : IStorageClient {
    private val testSessionStub = TestSessionServiceGrpcKt.TestSessionServiceCoroutineStub(channel)
    private val notificationStub =
        ProcessingNotificationServiceGrpcKt.ProcessingNotificationServiceCoroutineStub(channel)
    private val _notifications = MutableStateFlow<List<Notification>>(emptyList())
    private val defaultClientId = UUID.randomUUID().toString()

    init {
        CoroutineScope(Dispatchers.IO).launch {
            try {
                val request = SubscribeRequest.newBuilder().setUserId(defaultClientId).build()

                notificationStub.subscribe(request).collect { notification ->
                    _notifications.value += notification
                }

            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    override suspend fun getTestCases(paths: List<String>): Flow<List<Test>> = channelFlow {
        try {
            val collectedTests = mutableListOf<Test>()

            val request = DiscoverTestsRequest.newBuilder().setUserId(defaultClientId).addAllPaths(paths).build()
            val invocationResult = testSessionStub.discoverTests(request)
            if (invocationResult.success) {

                while (true) {
                    val notification = awaitNotification(invocationResult.requestId)
                    _notifications.value -= notification
                    if (notification.contentCase == Notification.ContentCase.TESTSDISCOVERYFINISHEDNOTIFICATION) {
                        collectedTests.updateOrAddTestCases(notification.testsDiscoveryFinishedNotification.testCasesList)
                        trySend(collectedTests.toList())
                        break;
                    }

                    collectedTests.updateOrAddTestCases(notification.testsDiscoveryUpdatedNotification.testCasesList)


                    trySend(collectedTests.toList())
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }

    }

    override suspend fun runSelectedTests(testCases: List<Test>): Flow<List<Test>> = channelFlow {
        try {
            val collectedTests = mutableListOf<Test>()
            collectedTests.addAll(testCases)

            val request = RunSelectedTestsRequest.newBuilder()
                .setUserId(defaultClientId)
                .addAllTestCases(testCases.toProto())
                .build()

            val invocationResult = testSessionStub.runSelectedTests(request)
            if (invocationResult.success) {

                while (true) {
                    val notification = awaitNotification(invocationResult.requestId)
                    _notifications.value -= notification
                    if (notification.contentCase == Notification.ContentCase.TESTSRUNFINISHEDNOTIFICATION) {
                        collectedTests.updateOrAddTestResults(notification.testsRunFinishedNotification.testResultsList)
                        trySend(collectedTests.toList())
                        break;
                    }

                    collectedTests.updateOrAddTestResults(notification.testsRunUpdatedNotification.testResultsList)


                    trySend(collectedTests.toList())
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    override suspend fun cancelCurrentOperation() {
        try {
            val request = CancelCurrentOperationRequest.newBuilder().build()
            testSessionStub.cancelCurrentOperation(request)

        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    private suspend fun awaitNotification(
        requestId: String
    ): Notification = coroutineScope {
        _notifications.flatMapLatest { list ->
            list.asFlow()
        }.filter { it.requestId == requestId }.first()
    }

    private fun TestRun.TestOutcome.toEntity(): Outcome = when (this) {
        TestRun.TestOutcome.None -> Outcome.None
        TestRun.TestOutcome.Failed -> Outcome.Failed
        TestRun.TestOutcome.Passed -> Outcome.Passed
        TestRun.TestOutcome.NotFound -> Outcome.NotFound
        TestRun.TestOutcome.Skipped -> Outcome.Skipped
        TestRun.TestOutcome.UNRECOGNIZED -> throw IllegalArgumentException("Unknown proto outcome value: $this")
    }

    private fun List<Test>.toProto(): List<TestCase> {
        val result = mutableListOf<TestCase>()
        for (testCase in this) {
            val testCaseProto =
                TestCase.newBuilder()
                    .setId(testCase.id)
                    .setSource(testCase.source)
                    .setDisplayName(testCase.displayName)
                    .setFullyQualifiedName(testCase.fullyQualifiedName)
                    .setExecutorUri(testCase.executorUri)
                    .build()

            result.add(testCaseProto)
        }
        return result
    }

    private fun MutableList<Test>.updateOrAddTestCases(testCases: List<TestCase>) {
        val existingById = this.associateBy { it.id }.toMutableMap()

        for (testCase in testCases) {
            existingById[testCase.id] = if (existingById[testCase.id] != null) Test(
                id = testCase.id,
                displayName = testCase.displayName,
                fullyQualifiedName = testCase.fullyQualifiedName,
                executorUri = testCase.executorUri,
                source = testCase.source,
                durationMs = existingById[testCase.id]!!.durationMs,
                outcome = existingById[testCase.id]!!.outcome,
                errorMessage = existingById[testCase.id]!!.errorMessage,
                errorStackTrace = existingById[testCase.id]!!.errorStackTrace
            )
            else Test(
                id = testCase.id,
                displayName = testCase.displayName,
                fullyQualifiedName = testCase.fullyQualifiedName,
                executorUri = testCase.executorUri,
                source = testCase.source,
                durationMs = -1,
                outcome = Outcome.None,
                errorMessage = "",
                errorStackTrace = ""
            )
        }
        removeAll { true }
        addAll(existingById.values.toList())
    }

    private fun MutableList<Test>.updateOrAddTestResults(testResults: List<TestResult>) {
        val existingById = this.associateBy { it.id }.toMutableMap()

        for (testResult in testResults) {
            existingById[testResult.testCaseId] = if (existingById[testResult.testCaseId] != null) Test(
                id = testResult.testCaseId,
                displayName = testResult.displayName,
                fullyQualifiedName = existingById[testResult.testCaseId]!!.fullyQualifiedName,
                executorUri = existingById[testResult.testCaseId]!!.executorUri,
                source = existingById[testResult.testCaseId]!!.source,
                durationMs = testResult.durationMs.toInt(),
                outcome = testResult.outcome.toEntity(),
                errorMessage = testResult.errorMessage,
                errorStackTrace = testResult.errorStackTrace
            )
            else Test(
                id = testResult.testCaseId,
                displayName = testResult.displayName,
                fullyQualifiedName = "",
                executorUri = "",
                source = "",
                durationMs = testResult.durationMs.toInt(),
                outcome = testResult.outcome.toEntity(),
                errorMessage = testResult.errorMessage,
                errorStackTrace = testResult.errorStackTrace
            )
        }
        removeAll { true }
        addAll(existingById.values.toList())
    }

    fun close() {
        channel.shutdown()
    }
}