import React from 'react'
import {CheckBox, CheckBoxClassName} from '../../../component/checkBox'
import {RadioBox, RadioBoxClassName} from '../../../component/radioBox'
import Lang from '../../../resource/Administration/AuthoritySetting/LangAPI'

function IsListOpen(HandleList, key, IsEnable=false, IsTopItem=false){
    let col = IsTopItem ? 'CloseList_MainItemIdx' : 'OpenList_ItemFuncKeys'
    let isOpen = ' is-open';
    let isClose = ''
    if(IsEnable){
        return isOpen
    }
    else if(IsTopItem){
        //default is open
        return HandleList && HandleList[col].indexOf(key) !==-1 ? isClose: isOpen
    }else if(!IsTopItem){
        //default is close
        return HandleList && HandleList[col].indexOf(key) ===-1 ? isClose: isOpen
    }else{
        return isClose
    }
}

function GetBindingValueObject(key, mainIdx, IsTopItem=false){
    return {FuncKey:key, MainIdx:mainIdx, IsTopItem:IsTopItem}
}

export default ({IsFullPage=false, ...props})=>{
    let datas = props.Datas;
    let rows = datas && datas.map((mainItems, mainIdx)=>{
            let _selectedCount = 0;
            let _totalFuncItems = mainItems.settings.length;
            let disabled = props.isUserAuthPage && mainItems.settings.filter(x=>{ return x.enable && x.isGroupSetting}).length === mainItems.settings.length;
            let checked = mainItems.settings.filter(x=>{ return x.enable }).length === mainItems.settings.length;
            let subItems = mainItems.settings.map((items, idx)=>{
                if(items.enable) _selectedCount++
                return <ListItems key={`Permission-${items.displayKey}-${idx}`} {...props.Functions} isUserAuthPage={props.isUserAuthPage} Datas={items} MainIdx={mainIdx}/>
            })
            let key = `Permission-MainItem-${mainIdx}`
            let isOpen = IsListOpen(props.Functions.HandleList, mainIdx, false, true);
        return(
            <div className={`list__item`} key={key}>
                <TopItem
                    Disabled={disabled}
                    SelectedCount={_selectedCount}
                    TotalFuncItems={_totalFuncItems}
                    DisplayText={mainItems.displayKey} 
                    Checked={checked} 
                    OnChange={props.Functions.EnableOrDisableFuncItem}
                    BindingValue={GetBindingValueObject(null, mainIdx, true)}
                    OpenOrCloseList={props.Functions.OpenOrCloseList.bind(null, mainIdx)}
                />
                <div className={`list list--2nd${isOpen}`}>
                    {subItems}
                </div>
            </div>
        )
    })

    let fullPageClass = IsFullPage ? ' list--fullpage' : '';
    let mainClass = IsFullPage ? 'panel__row-list' : 'popup__row';
    return (
        <div className={mainClass}>
            <div className={`list list--1st list--tighten${fullPageClass}`}>
                {rows}
            </div>
        </div>
    )
}

const TopItemDisplayText = (props)=>{
    return(
        <span className="text">
            {props.DisplayText} ( <span className="text text-highlight">{props.SelectedCount}</span> / {props.TotalFuncItems} )
        </span>
    )
}

const TopItem = (props)=>{
    return(
        <div className="list__btn">
            <CheckBox
                Disabled={props.Disabled}
                Checked={props.Checked}
                HandleClick={props.OnChange}
                BindingValue={props.BindingValue}
                DisplayElementText={<TopItemDisplayText DisplayText={props.DisplayText} SelectedCount={props.SelectedCount} TotalFuncItems={props.TotalFuncItems}/>}
                Value={props.Checked}
            />
            <div className="newbtn" onClick={props.OpenOrCloseList}>
                <i className="icon icon-arrow_drop_down"></i>
            </div>
        </div>
    )
}

const ListItems = (props)=>{
    let datas = props.Datas;        
    let rows = null;
    if(datas.funcKeys){
        rows = <FunctionKeys {...props} Datas={datas} />
    }else if(datas.subFuncKeys){
        rows = <SubFunctionKeys {...props}  Datas={datas}/>
    }else{
        rows = <Item {...props}  Datas={datas} />
    }
    
    return rows;    
}

const Item = (props)=>{
    let datas = props.Datas;
    return (
        <div className={`list__item`}>
            <div className="list__title">
                <span className="text">{datas.displayKey}</span>
            </div>
            <AllowCheckbox 
                Disabled={props.isUserAuthPage && datas.isGroupSetting}
                DisplayText={Lang.getLang(window.LangCode, "lbl_allowed")}
                Checked={datas.enable ? true : false} 
                OnChange={props.EnableOrDisableFuncItem}
                BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
            />
        </div>
    )
}

const FunctionKeys = (props)=>{
    let datas = props.Datas;
    let rows = datas.funcKeys && datas.funcKeys.map(items=>{
        let key = `Permission-FuncList-${items.funcKey}`
        if(items.funcKeys){
            return <FunctionKeysSubList key={key} {...props} Datas={items} />
        }else if(items.subFuncKeys){
            return <SubFunctionKeysList key={key} {...props} Datas={items} />
        }else{
            return <ListItemsWithButtons key={key} {...props} Datas={items} />
        }
    });
    let isOpen = IsListOpen(props.HandleList, datas.funcKey, datas.enable);
    return (
        <div className={`list__item`}>
            <div className="list__title">
                <span className="text">{datas.displayKey}</span>
            </div>
            <AllowCheckbox 
                Disabled={props.isUserAuthPage && datas.isGroupSetting}
                DisplayText={Lang.getLang(window.LangCode, "lbl_allowed")}
                Checked={datas.enable ? true : false} 
                OnChange={props.EnableOrDisableFuncItem}
                BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
            />
            <div className={`list${isOpen}`}>
                {rows}
            </div>
        </div>
    )
}

const FunctionKeysSubList = (props)=>{
    let datas = props.Datas;
    let rows = datas.funcKeys && datas.funcKeys.map(items=>{
        let key = `Permission-FuncList-${items.funcKey}`
        if(items.funcKeys){
            return <FunctionKeysSubList key={key} {...props}  Datas={items}/>
        }else if(items.subFuncKeys){
            return <SubFunctionKeysList key={key} {...props}  Datas={items}/>
        }else{
            return <ListItemsWithButtons key={key} {...props}  Datas={items} />
        }
    });
    
    let isOpen = IsListOpen(props.HandleList, datas.funcKey, datas.enable);
    return(
        <div className={`list__item`}>
            <div className="list__btn">
                <AllowCheckbox 
                    Disabled={props.isUserAuthPage && datas.isGroupSetting}
                    DisplayText={datas.displayKey} 
                    Checked={datas.enable ? true : false} 
                    OnChange={props.EnableOrDisableFuncItem}
                    BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
                />
            </div>
            <div className={`list${isOpen}`}>
                {rows}
            </div>
        </div>
    )
}

const ListItemsWithButtons = (props)=>{
    let datas = props.Datas;
    return(
        <div className={`list__item`}>
            <div className="list__btn">
                <AllowCheckbox 
                    Disabled={props.isUserAuthPage && datas.isGroupSetting}
                    DisplayText={datas.displayKey} 
                    Checked={datas.enable ? true : false} 
                    OnChange={props.EnableOrDisableFuncItem}
                    BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
                />
            </div>
        </div>
    )
}

function IsSelectSubOption(OptionsIdx, userCheckedIndex, groupCheckedIndex){
    let result = {isGroupSetting:false, checked:false};
    if(groupCheckedIndex && groupCheckedIndex.indexOf(OptionsIdx)!==-1){
        result.isGroupSetting = true;
        result.checked = true;
    }else if(userCheckedIndex && userCheckedIndex.indexOf(OptionsIdx)!==-1){
        result.isGroupSetting = false;
        result.checked = true;
    }

    return result;
}

const SubFunctionKeys = (props)=>{
    let datas = props.Datas;
    let isOpen = IsListOpen(props.HandleList, datas.funcKey, datas.enable);

    return(
        <div className={`list__item`}>
            <div className="list__title">
                <span className="text">{props.Datas.displayKey}</span>
            </div>
            <AllowCheckbox 
                Disabled={props.isUserAuthPage && datas.isGroupSetting}
                DisplayText={Lang.getLang(window.LangCode, "lbl_allowed")}
                Checked={datas.enable ? true : false} 
                OnChange={props.EnableOrDisableFuncItem}
                BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
            />
            <SubOptions {...props} IsOpen={isOpen}/>
        </div>
    )
}

const SubFunctionKeysList  = (props)=>{
    let datas = props.Datas;
    let isOpen = IsListOpen(props.HandleList, datas.funcKey, datas.enable);

    return(
        <div className={`list__item`}>
            <div className="list__btn">
                <AllowCheckbox
                    Disabled={props.isUserAuthPage && datas.isGroupSetting}
                    DisplayText={datas.displayKey} 
                    Checked={datas.enable ? true : false} 
                    OnChange={props.EnableOrDisableFuncItem}
                    BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
                />
            </div>
            <SubOptions {...props} IsOpen={isOpen}/>
        </div>
    )
}

const SubOptions = (props)=>{
    let datas = props.Datas;
    let disableAll = datas.inputBoxType === 'radio' && props.isUserAuthPage && datas.subFuncKeys.filter((d, idx)=>{
        return IsSelectSubOption(idx, datas.userCheckedIndex, datas.groupCheckedIndex).isGroupSetting;
    }).length > 0;
    
    let rows = datas.subFuncKeys.map((item, idx)=>{
        let key = `Permission-${item.funcKey}-suboptions-${item.SubFunctionKey}`
        let checkedResult = IsSelectSubOption(idx, datas.userCheckedIndex, datas.groupCheckedIndex);
        return(
            <SubOption key={key}
                Disabled={disableAll || (props.isUserAuthPage && checkedResult.isGroupSetting)}
                InputBoxType={datas.inputBoxType}
                Checked={checkedResult.checked}
                DisplayText={item.displayKey} 
                BindingValue={GetBindingValueObject(datas.funcKey, props.MainIdx)}
                HandleClick={props.EnableOrDisableSubOptionsItem}
                ParentFunctionKey={datas.funcKey}
                OptionIdx={idx}
            />
        )
    })
    return(
        <div className={`list list--horizontal${props.IsOpen}`}>
            {rows}
        </div>
    )
}

const SubOption = (props)=>{
    let content = null;
    if(props.InputBoxType==='radio'){
        content = <RadioBox
                        Disabled={props.Disabled}
                        ClassName={RadioBoxClassName.checkbox_checked}
                        Checked={props.Checked}
                        DisplayText={props.DisplayText}
                        RadioGroup={props.ParentFunctionKey}
                        Value={props.OptionIdx}
                        BindingValue={props.BindingValue}
                        HandleClick={props.HandleClick}
                    />
    }else if(props.InputBoxType==='checkbox'){
        content =  <CheckBox 
                        Disabled={props.Disabled}
                        ClassName={CheckBoxClassName.checkbox_checked}
                        Checked={props.Checked}
                        HandleClick={props.HandleClick}
                        BindingValue={props.BindingValue}
                        DisplayText={props.DisplayText}
                        Value={props.OptionIdx}
                    />
    }
    return(
        <div className="list__item">
            <div className="list__btn">
                {content}    
            </div>
        </div>
    )
}

const AllowCheckbox = (props)=>{
    let displayText = props.DisplayText ? props.DisplayText : "Allowed"
    return(
        <div className="list__btn">
            <CheckBox
                Disabled={props.Disabled}
                ClassName={CheckBoxClassName.checkbox_checked}
                Checked={props.Checked}
                HandleClick={props.OnChange}
                BindingValue={props.BindingValue}
                DisplayText={displayText}
                Value={props.Checked}
            />
        </div>
    )
}