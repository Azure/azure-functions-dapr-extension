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
 *  Attribute to specify parameters for the Dapr binding trigger.
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
@CustomBinding(direction = "in", name = "daprBindingTriggerMessage", type = "daprBindingTrigger")
public @interface DaprBindingTrigger {
    /**
     * Name of the Dapr trigger.
     */
    String bindingName() default "";
}
