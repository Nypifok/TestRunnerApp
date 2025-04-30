package testrunner.app.configuration

import kotlinx.serialization.json.Json

object ConfigurationManager {

    private var appConfigName = ""

    val config: AppConfiguration by lazy {
        loadConfig()
    }

    private fun loadConfig(): AppConfiguration {

        val env = System.getenv("APP_ENV") ?: "dev"

        appConfigName = if (env == "prod") {
            "config.prod.json"
        } else {
            "config.dev.json"
        }
        val classLoader = this.javaClass.classLoader
        val inputStream = classLoader.getResourceAsStream(appConfigName)
        if (inputStream != null) {
            val jsonString = inputStream.bufferedReader().use { it.readText() }
            return Json.decodeFromString<AppConfiguration>(jsonString)
        } else {
            throw IllegalArgumentException("Resource not found!")
        }
    }
}