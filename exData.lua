local exData = class("exData")
local rawget = rawget
local rawset = rawset
local keyEncryption = "XGR"

local meta_exData = {
    __newindex = function(c, key, value)
        error("donot to modify exDataCity data")
    end
}

local callbacks = {}
local nextLifeId = 1

function exData.init(data)
    local root = {}
    setmetatable(root, meta_exData)
    if (data) then
        exData.initValue(root, data)
    end
    return root
end

function exData.initValue(root, data)
    if type(data) ~= "table" then
        return
    end
    for key, value in pairs(data) do
        local keyTitle = keyEncryption..key
        if type(value) == "table" then
            local child = exData.init(value)
            rawset(root, keyTitle, child)
        else
            rawset(root, keyTitle, value)
        end
    end
end

function exData.get(root, ...)
    local params = table.pack(...)
    local result = nil
    for index, key in ipairs(params) do
        local keyTitle = keyEncryption..key
        result = rawget(root, keyTitle)
        if (not result) then
            break
        end
        root = result
    end
    return result
end

function exData.set(root, key, value, ...)
    local keyTitle = keyEncryption..key
    local params = table.pack(...)
    local result = nil
    for index, path in ipairs(params) do
        result = rawget(root, keyEncryption..path)
        if (not result) then
            break
        end
        root = result
    end
    if (result) then
        rawset(result, keyTitle, value)
    end
    for _, callBackInfo in pairs(callbacks) do
        local index = params.n + 1 < callBackInfo.params.n and params.n + 1 or callBackInfo.params.n
        local needCallBack = true
        for i = 1, index, 1 do
            local param = i <= params.n and params[i] or key
            local callBackParam = callBackInfo.params[i]
            if (param ~= callBackParam) then
                needCallBack = false
                break
            end
        end
        if (needCallBack) then
            callBackInfo.callBack()
        end
    end
end

exData.tabAddListener = _({
    s1 = function(c, callBack, ...)
        local params = table.pack(...)
        c.nextLifeId = nextLifeId
        nextLifeId = nextLifeId + 1
        callbacks[c.nextLifeId] = {params = params, callBack = callBack}
    end,
    event = g_t.empty_event,
    final = function(c)
        callbacks[c.nextLifeId] = nil
    end,
})

return exData

