/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.microsoft.azure.functions.dapr.annotation;

import java.lang.annotation.Retention;
import java.lang.annotation.Target;

import com.microsoft.azure.functions.annotation.CustomBinding;

import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.ElementType;

/**
 *  Attribute to specify parameters for the Dapr service invocation trigger.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target({ ElementType.PARAMETER, ElementType.METHOD })
@CustomBinding(direction = "in", name = "daprServiceInvocationTriggerMessage", type = "daprServiceInvocationTrigger")
public @interface  DaprServiceInvocationTrigger {
    /**
     * Name of the method on a remote Dapr App.
     */
    String methodName() default "";
}
