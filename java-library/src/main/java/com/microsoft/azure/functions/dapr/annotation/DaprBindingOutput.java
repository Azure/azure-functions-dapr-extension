/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.microsoft.azure.functions.dapr.annotation;
import java.lang.annotation.Retention;
import java.lang.annotation.Target;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.ElementType;

@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
/**
 * Attribute to specify parameters for the Dapr output bindings.
 */
public @interface DaprBindingOutput {
    /**
     * The variable name used in function.json.
     */
    String name();

    /**
     * Dapr runtime endpoint.
     */
    String daprAddress() default "";

    /**
     * Secret store to get the secret from.
     */
    String secretStoreName();

    /**
     * Identifying the name of the secret to get.
     */
    String key() default "";

    /**
     * An array of metadata properties in the form "key1=value1&amp;key2=value2".
     */
    String metadata() default "";
}
