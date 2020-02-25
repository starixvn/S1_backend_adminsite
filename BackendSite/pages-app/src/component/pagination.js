import React from 'react'
import {DropdownBox} from './dropdownBox'

const _rowsPerPage = [20, 50, 200]
export const RowsPerPage = (props)=>{
    let Items = props.Items ? props.Items : _rowsPerPage;
    return(
        <DropdownBox 
            OnSelectItem={props.OnSelectItem} 
            ColName={props.ColName} 
            Items={Items}
            DropDownBoxClassName="newdropdown-container newdropdown-container--hor"
            SelectValue={props.SelectValue}
            DefaultIsFirstOptionValue={true}
            />
    )    
}

export const DeviceType = {
    'Desktop':'Desktop',
    'Mobile':'Mobile'
}

export const Pagination = (props)=>{
    return(
        <div className="newpagination">
            <SwitchPageBtn Icon="icon icon-first_page" BtnStatus={PageBtnStatus(props.CurrentPage, "<<", props.TotalPages)} SwitchPage={props.GoToFirstPage}/>
            <SwitchPageBtn Icon="icon icon-keyboard_arrow_left" BtnStatus={PageBtnStatus(props.CurrentPage, "<", props.TotalPages)} SwitchPage={props.GoToPrevPage}/>
            {PageBtnsByDeviceType(props)}
            <SwitchPageBtn Icon="icon icon-keyboard_arrow_right" BtnStatus={PageBtnStatus(props.CurrentPage, ">", props.TotalPages)} SwitchPage={props.GoToNextPage}/>
            <SwitchPageBtn Icon="icon icon-last_page" BtnStatus={PageBtnStatus(props.CurrentPage, ">>", props.TotalPages)} SwitchPage={props.GoToLastPage}/>
        </div>
    )
}

const PageBtnsByDeviceType = (props)=>{
    switch(props.DeviceType){
        case DeviceType.Mobile:
            return <MobilePagination CurrentPage={props.CurrentPage} TotalPages={props.TotalPages}/>
        default:
            return <DesktopPagination CurrentPage={props.CurrentPage} TotalPages={props.TotalPages} GoToSelectedPage={props.GoToSelectedPage}/>
    }
}

const MobilePagination = (props)=>{
    return(
        <div className="newpagination__page">
            <div className="textfield">
                <input type="text" className="textfield__input-center" value={props.CurrentPage}/>
            </div>
            <span className="text">of {props.TotalPages}</span>
        </div>
    )    
}

const DesktopPagination = (props)=>{
    return(
        <PageBtns CurrentPage={props.CurrentPage} TotalPages={props.TotalPages} GoToSelectedPage={props.GoToSelectedPage}/>
    )    
}

const PageBtnStatus = (currentPage, displayText, totalPages)=> {
    const def_state = {
        'disabled':'is-disabled',
        'current':'is-current',
        'default':''
    }

    let status = def_state.default;
        switch(displayText){
            case "...":
                status = def_state.disabled
                break;
            case currentPage:
                status = def_state.current
                break;
            case "<":
                status = currentPage === 1 ? def_state.disabled : def_state.default;
                break;
            case "<<":
                    status = currentPage === 1 ? def_state.disabled : def_state.default;
                    break;
            case ">":
                status = currentPage === totalPages ? def_state.disabled : def_state.default;
                break;
            case ">>":
                status = currentPage === totalPages ? def_state.disabled : def_state.default;
                break;
            default:
                status = def_state.default
                break;
    }

    return status;
}

const PageBtns = (props)=>{
    let pageRanges = PageRange(props.CurrentPage, props.TotalPages);
    let buttons = pageRanges.map((displayText, idx)=>{
        let key = `PageRange-${idx}`;
        let status = PageBtnStatus(props.CurrentPage, displayText, props.TotalPages);

        return <PageBtn key={key} Text={displayText} BtnStatus={status} GoToSelectedPage={props.GoToSelectedPage}/>
    })

    return(
        <div className="newpagination__page">
            {buttons}
        </div>
    )    
}

const PageBtn = (props)=>{
    return(
        <div className={`newbtn newbtn-tertiary ${props.BtnStatus}`} onClick={props.GoToSelectedPage.bind(null, props.Text)}>
            <span className="text">{props.Text}</span>
        </div>
    )
}

const SwitchPageBtn = (props)=>{
    return(
        <div className={`newbtn newbtn-tertiary ${props.BtnStatus}`} onClick={props.SwitchPage}>
            <i className={props.Icon}></i>
        </div>
    )
}

function PageRange(currentPage, totalPages) {
    let pairs = 2, range = [1]

    if (totalPages <= 1) return range;

    let range_of_head = currentPage - pairs;
    let range_of_tail = currentPage + pairs;
    for (let i = range_of_head ; i <= range_of_tail; i++) {
        if (i < totalPages && i > 1) range.push(i);
    }
    range.push(totalPages);

    let tmp, rangeWithDots = []
    for (let i of range) {
        if (tmp) {
            if (i - tmp === 2) {
                rangeWithDots.push(tmp + 1);
            } else if (i - tmp !== 1) {
                rangeWithDots.push('...');
            }
        }
        rangeWithDots.push(i);
        tmp = i;
    }

    return rangeWithDots;
}