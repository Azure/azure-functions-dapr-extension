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
public @interface DaprTopicTrigger {
    /**
     * The variable name used in function.json.
     */
    String name();

    /**
     * Pub/Sub name.
     */
    String pubSubName();

    /**
     * Topic name. If unspecified the function name will be used.
     */
    String topic() default "";

    /**
     * Route for the trigger. If unspecified the topic name will be used.                              
     */
    String route() default "";
}
