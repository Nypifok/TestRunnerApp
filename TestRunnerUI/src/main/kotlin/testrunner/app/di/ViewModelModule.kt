package testrunner.app.di

import kotlinx.coroutines.CoroutineScope
import org.koin.dsl.module
import testrunner.app.viewmodel.TestExplorerViewModel

val viewModelModule = module {
    factory { (viewModelScope: CoroutineScope) ->
        TestExplorerViewModel(model = get(), viewModelScope = viewModelScope)
    }

}