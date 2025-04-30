package testrunner.app.di

import org.koin.dsl.module

val appModule = module {
    single { testrunner.app.configuration.ConfigurationManager }
}