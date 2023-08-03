/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.microsoft.azure.functions.dapr.annotation;

import com.microsoft.azure.functions.annotation.CustomBinding;
import java.lang.annotation.Retention;
import java.lang.annotation.Target;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.ElementType;

/**
 *  Attribute to specify parameters for the Dapr invoke output binding.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "out", name = "daprInvokeOutputMessage", type = "daprInvoke")
public @interface DaprInvokeOutput {
    /**
     * Dapr runtime endpoint.
     */
    String daprAddress() default "";

    /**
     * Dapr app name to invoke.
     */
    String appId() default "";

    /**
     * Method name of the app to invoke.
     */
    String methodName() default "";

    /**
     * HTTP verb for invoking the app.
     */
    String httpVerb() default "POST";
}
