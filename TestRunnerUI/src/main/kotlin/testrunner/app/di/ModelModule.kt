package testrunner.app.di

import io.grpc.LoadBalancerRegistry
import io.grpc.ManagedChannel
import io.grpc.ManagedChannelBuilder
import io.grpc.internal.DnsNameResolverProvider
import io.grpc.internal.PickFirstLoadBalancerProvider
import org.koin.dsl.module
import org.koin.dsl.onClose
import testrunner.app.implementations.StorageClient
import testrunner.app.model.TestSessionModel
import testrunner.app.model.contract.IStorageClient

fun modelModule(port: Int) = module {
    LoadBalancerRegistry.getDefaultRegistry().register(PickFirstLoadBalancerProvider())
    single { port }
    single<ManagedChannel> {
        val p = get<Int>()
        ManagedChannelBuilder
            .forAddress("localhost", p)
            .nameResolverFactory(DnsNameResolverProvider())
            .usePlaintext()
            .build()
    } onClose { channel ->
        channel?.shutdown()

        if (!channel?.awaitTermination(5, java.util.concurrent.TimeUnit.SECONDS)!!) {
            channel.shutdownNow()
        }
    }
    single<IStorageClient> { StorageClient(get()) }
    single { TestSessionModel(get()) }
}