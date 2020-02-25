import React, {Component} from 'react';

export const dropdownBoxClassName = {
    'panel__col':'panel__col',
    'newdropdown_container_hor_dark':'newdropdown-container newdropdown-container--hor newdropdown-container--dark',
}

export const dropdownBoxSubClassName = {
    default:'',
    TextInput:'newdropdown--text-input'
}

export const CurrentSelectedElemType = {
    default:'Button',
    InputBox:'InputBox'
}

export class DropdownBox extends Component{
    constructor(props){
        super(props)
        this.state = {isOpen:false}
        this.ClickDropdown = this.ClickDropdown.bind(this)
        this.handleClickOutside = this.handleClickOutside.bind(this)
        document.addEventListener('mousedown', this.handleClickOutside);
    }
    handleClickOutside(e) {
        if (this.wrapper && !this.wrapper.contains(e.target)) {
            this.setState({isOpen:false});
            if(this.props.OnLeaveDropdown){
                this.props.OnLeaveDropdown(this.props.DataColumn);
            }
        }
    }
    SetOuterDivRef=(node)=>{
        this.wrapper = node;
    }
    ClickDropdown(){
        if(!this.props.IsReadOnly){
            this.setState({isOpen: !this.state.isOpen})
        }
    }
    SelectItem(v){
        if(!this.props.IsReadOnly){
            this.setState({isOpen:false})
            if(this.props.OnSelectItem !== undefined){
                this.props.OnSelectItem(v, this.props.DataColumn);
            }
        }
    }
    InputBoxOnChange=(e)=>{
        if(!this.props.IsReadOnly){
            this.setState({isOpen:true})
            this.props.OnSelectItem(e.target.value, this.props.DataColumn, true);
        }
    }
    render(){
        let colName = this.props.ColName;
        let mainKey = colName ? colName : this.props.MainKey;
        let rows = this.props.ShowPreloader ? <Preloader /> : this.props.Items && this.props.Items.map((data, idx)=>{
            let displayText = typeof(data)==='object' ? data.DisplayText : data;
            return (
                <DropdownBoxItem key={`${mainKey}-${data}-${idx}`} OnClick={this.SelectItem.bind(this, data)} Data={displayText}/>
            )
        })

        let _isReadOnly = this.props.IsReadOnly ? ' is-readonly' : '';
        let _isEdited = this.props.IsEdited ? ' is-edited':'';
        let _isDisabled = this.props.IsDisabled ? ' is-disabled' : '';
        let _selectedValue = this.props.SelectValue !== null && this.props.SelectValue !== undefined && typeof(this.props.SelectValue) === 'object' ? this.props.SelectValue.DisplayText : this.props.SelectValue;
        
        let _isOpen = this.state.isOpen ? " is-open" : "";
        
        let currnetSelectedElem = (
            <DropdownBoxCurrentSelectElem 
                ElemType={this.props.CurrentSelectedElem.ElemType} 
                Placeholder={this.props.CurrentSelectedElem.DefaultDisplayText} 
                SelectedValue={_selectedValue}
                InputBoxOnChange={this.InputBoxOnChange}
                Key={this.props.CurrentSelectedElem.Key}
            />
        );

        let elemTypeClass = this.props.CurrentSelectedElem.ElemType === CurrentSelectedElemType.InputBox ? ' newdropdown--text-input':'';

        let result = <div></div>;
        if(this.props.HasTitle){
            result = (
                <div ref={this.SetOuterDivRef} className={this.props.DropDownBoxClassName}>
                    <span className="text-title">{colName}</span>
                    <div onClick={this.ClickDropdown} className={`newdropdown${elemTypeClass}${this.props.DropDownBoxSubClassName? ' '+this.props.DropDownBoxSubClassName:''}${_isEdited}${_isReadOnly}${_isOpen}${_isDisabled}`}>
                            {currnetSelectedElem}
                        <div className="newdropdown__list">
                            {rows}
                        </div>
                    </div>
                </div>
            );
        }else{
            result = (
                <div ref={this.SetOuterDivRef} onClick={this.ClickDropdown} className={`newdropdown${this.props.DropDownBoxSubClassName? ' '+this.props.DropDownBoxSubClassName:''}${_isEdited}${_isReadOnly}${_isOpen}${_isDisabled}`}>
                    {currnetSelectedElem}
                    <div className="newdropdown__list">
                        {rows}
                    </div>
                </div>
            );
        }

        return result;
    }
}

const DropdownBoxCurrentSelectElem = ({ElemType=CurrentSelectedElemType.default, Placeholder='', ...props})=>{
    let result = null;
    if(ElemType === CurrentSelectedElemType.InputBox){
        result = (
            <div className={`textfield`}>
                <input key={props.Key} type="text" className="textfield__input" title={props.SelectedValue} placeholder={Placeholder} value={props.SelectedValue} onChange={props.InputBoxOnChange}/>
                <div className="newbtn">
                    <i className="icon icon-arrow_drop_down"></i>
                </div>
            </div>
        );
    }else{
        result = (
            <div className="newbtn">
                <span className="text" title={props.SelectedValue}>{props.SelectedValue}</span>
                <i className="icon icon-arrow_drop_down"></i>
            </div>
        );
    }

    return result;
}

DropdownBox.defaultProps = {
    DropDownBoxClassName:dropdownBoxClassName.panel__col,
    DropDownBoxSubClassName:dropdownBoxSubClassName.default,
    DefaultValue: false,
    HasTitle:true,
    IsEdited:false,
    IsReadOnly:false,
    CurrentSelectedElem:{
        ElemType:CurrentSelectedElemType.default,
        DefaultDisplayText:''
    }
}

const DropdownBoxItem = (props)=>{
    return(
        <div onClick={props.OnClick} className="newdropdown__item">
            <div className="newbtn">
                <span className="text">{props.Data}</span>
            </div>
        </div>
    )    
}

function IsSelectAll(options, selectedItem){
    let selected_all = true;
    for(let i in options){
        if(Number(i)===0) continue;        
        if(selectedItem.find(x=>x.Value===options[i].Value)===undefined){
            selected_all = false;
            break;
        }
    }
    
    return selected_all;
}

export class MutiSelectDropdownBox extends Component{
    constructor(props){
        super(props)
        let selectedItems = [];
        if(this.props.Items){
            selectedItems = [...this.props.Items];
        }
        if(this.props.SetupDefaultValue){
            this.props.SetupDefaultValue(selectedItems.map(x=>{return x.Value}))
        }

        this.state = {selectedItems:selectedItems, isOpen:false}
        document.addEventListener('mousedown', this.handleClickOutside);
    }
    handleClickOutside=(e)=>{
        if (this.wrapper && !this.wrapper.contains(e.target) && this.state.isOpen) {
            this.setState({isOpen:false})            
        }
    }
    SetWrapperRef=(node)=>{
        this.wrapper = node;
    }
    ClickDropdown=()=>{
        this.setState({isOpen: !this.state.isOpen})
    }
    handleSelectAll = (IsSelect, currentSelectedItem, allSelectItems)=>{
        let elem_all = this.props.Items[0];
        let result = allSelectItems;
        if(IsSelect){            
            let isSelectAll = currentSelectedItem.Value === elem_all.Value || IsSelectAll(this.props.Items, allSelectItems);
            if(isSelectAll){
                result = this.props.Items;
            }
        }else{
            if(currentSelectedItem.Value===elem_all.Value){
                result = [];
            }else{
                let idx_all = allSelectItems.findIndex((x)=>x.Value === elem_all.Value);
                if(idx_all !== -1){
                    result.splice(idx_all, 1);
                }
            }
        }
        
        return result
    }
    SelectItem(v){
        const optionsHasAll = this.props.OptionsHasAll;        
        let selectedItems = this.state.selectedItems;
        let idx = selectedItems.findIndex((x)=>x.Value === v.Value);

        if(idx === -1){
            selectedItems.push(v);
        }else{
            selectedItems.splice(idx, 1);
        }

        if(optionsHasAll){
            let isSelect = idx===-1;
            selectedItems = this.handleSelectAll(isSelect, v, selectedItems)
        }
        
        this.props.OnSelectItem(selectedItems.map(x=>{return x.Value}), this.props.DataColumn);
        this.setState({selectedItems:selectedItems})        
    }
    render(){
        let colName = this.props.ColName
        let rows = this.props.Items && this.props.Items.map((data, idx)=>{
            let checked = this.state.selectedItems.find(v => v.Value === data.Value) !== undefined;
            return <MutiSelectDropdownBoxItem {...data} key={`${colName}-${data.DisplayText}-${idx}`} OnSelectItem={this.SelectItem.bind(this, data)} Checked={checked}/>
        })
        
        return(
            <div ref={this.SetWrapperRef} className={this.props.DropdownWrapperClass}>
                <span className="text-title">{colName}</span>
                <div onClick={this.ClickDropdown} className={`newdropdown${this.state.isOpen ? " is-open" : ""}`}>
                    <div className="newbtn">
                        <span className="text">{this.props.OptionsHasAll && IsSelectAll(this.props.Items, this.state.selectedItems) ? this.props.Items[0].DisplayText : this.state.selectedItems.map(x=>{return x.DisplayText}).join(', ')}</span>
                        <i className="icon icon-arrow_drop_down"></i>
                    </div>
                    <div className="newdropdown__list">
                        {rows}
                    </div>
                </div>
            </div>
        )
    }
}

const MutiSelectDropdownBoxItem = (props)=>{
    return(
        <div className="newdropdown__item">
            <label className="checkbox">
                <input type="checkbox" onChange={props.OnSelectItem} checked={props.Checked}/>
                <i className="icon icon-checkbox"></i>
                <span className="text">{props.DisplayText}</span>
            </label>
        </div>
    )
}

const Preloader = ()=>{
    return(
        <div className="preloader">
            <div className="preloader__spiner">
                <div className="preloader__circles">
                    <div className="preloader__circle-1"></div>
                    <div className="preloader__circle-2"></div>
                </div>
            </div>
        </div>
    )
}