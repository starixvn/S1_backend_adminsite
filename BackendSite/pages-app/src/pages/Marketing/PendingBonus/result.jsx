import React from 'react'
import moment from 'moment';
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

const defPromotionType = {
    1:"First Deposit",
    2:"Re-Deposit",
    3:"FreeBet",
    4:"Special"
}

const SpecialItemType = {
	actionBtn: "actionBtn"
};

const dataColumnDefs = ()=>{
    return [
        {dataColumn:'bonusCode', columnTitle:'BonusCode'},
        {
            dataColumn:'promotionType',
            columnTitle:'PromotionType',
            body:{
                format:PromotionTypeFormat
            }
        },
        {
            dataColumn:'siteName', 
            columnTitle:'SiteName'
        },
        {dataColumn:'userName', columnTitle:'UserName'},
        {
            dataColumn:'lockedAmount',
            columnTitle:'LockedAmount',
            header:{
                attr:{
                    class:"text-right"
                }
            },
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'bonusAmount',
            columnTitle:'BonusAmount',
            header:{
                attr:{
                    class:"text-right"
                }
            },
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'bonus',
            columnTitle:'Bonus',
            header:{
                attr:{
                    class:"text-right"
                }
            },
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'rollover',
            columnTitle:'Rollover',
            header:{
                attr:{
                    class:"text-right"
                }
            },
            body:{
                attr:{
                    class:"text-right"
                },
                format:numberFormat
            }
        },
        {
            dataColumn:'expiryTime', 
            columnTitle:'ExpiryTime',
            body:{
                format:dateFormat
            }
        },
        {
            dataColumn:'createTime', 
            columnTitle:'CreateTime',
            body:{
                format:dateFormat
            }
        },
		{
			columnTitle: "Action",
			header: {
				attr: {
					className: "width-85"
				}
			},
			body: {
				SpecialItem: SpecialItemType.actionBtn
			}
		}
    ]
}


export default (props)=>{
    const columnDefs = dataColumnDefs();
    let headerRows = [];
    
    columnDefs.forEach((element, idx) => {
        headerRows.push(
            <th key={`th-${element.dataColumn}-${idx}`} scope="col" className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
                {element.columnTitle}
            </th>
        );
    });

    let bodyRows = props.Datas.map((data, d_idx) => {
		let row = columnDefs.map((element, c_idx) => {
			let body_content = null;
			if (element.body && element.body.SpecialItem) {
				if (element.body.SpecialItem === SpecialItemType.actionBtn) {
					body_content = (
						<ActionButtons
                            ClickTransfer={props.ClickTransfer.bind(null, d_idx)}
                            ClickCancel={props.ClickCancel.bind(null, d_idx)}
                            Processing={props.processing}
						/>
					);
				}
			} else {
				let subData = {};
				if (element.body && element.body.subData) {
					element.body.subData.forEach(s => {
						subData[s] = data[s];
					});
				}

				body_content =
					element.body && element.body.format
						? element.body.format(data[element.dataColumn], subData)
						: data[element.dataColumn];
			}

			return (
				<td
					key={`td-${element.dataColumn}-${c_idx}`}
					className={
						element.body &&
						element.body.attr &&
						element.body.attr.class
							? element.body.attr.class
							: ""
					}>
					{body_content}
				</td>
			);
		});

		return <tr key={`result-body-${d_idx}`}>{row}</tr>;
	});
    return(
        <div className="table">
            <table className="table table-striped">
                <thead>
                    <tr>
                    {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {bodyRows}
                </tbody>
            </table>
        </div>
    )
}

const dateFormat = (createTime)=>{
    return moment(createTime).format('l LTS');
}

const numberFormat = (number)=>{
    return number_format(number_toFixed(number, 2));
}

const PromotionTypeFormat = (type)=>{
    return defPromotionType[type];
}

const ActionButtons = props => {
	return (
		<>
			<button className="btn btn-add" onClick={props.ClickTransfer} disabled={props.Processing}>
				Transfer
			</button>
			{/*<button className="btn btn-rejected" onClick={props.ClickCancel} style={{marginLeft:'10px'}} disabled={props.Processing}>*/}
			{/*	Cancel*/}
			{/*</button>*/}
		</>
	);
};