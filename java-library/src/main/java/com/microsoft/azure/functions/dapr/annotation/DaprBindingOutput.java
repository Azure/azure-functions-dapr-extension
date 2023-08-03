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
 * Attribute to specify parameters for the Dapr output bindings.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "out", name = "daprBindingOutputMessage", type = "daprBinding")
public @interface DaprBindingOutput {
    /**
     * Dapr runtime endpoint.
     */
    String daprAddress() default "";

    /**
    * Name of the Dapr binding, should be same as the name specified in the Dapr component YAML.
     */
    String bindingName() default "";

    /**
     * Configured operation.
     */
    String operation() default "";
}
