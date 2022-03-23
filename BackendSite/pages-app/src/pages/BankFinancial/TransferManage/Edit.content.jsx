import React from "react";
import InputBox from "./Component/InputBox";
import UploadBox from "./Component/UploadBox";
import DateTimePickerElem from "./Component/DateTimePicker";
import SelectBox from "./Component/SelectBox";
import RadioBox from "./Component/RadioBox";
import TextBox from "./Component/TextBox";
import moment from "moment";
import { initChannelType, initTransferType,initTransactionType } from "./defCols";
import {number_format, number_toFixed} from '../../../module/Formatting/NumberFormat';

export default ({ state, action, processing,isAdd }) => {	
	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor"></div>
			<div className="panel level-1 mt-20">
				<div
					className="panel-title no-arrow"
					style={{ justifyContent: "space-between" }}>
					<span style={{ color: "white" }}>Transfer Info</span>
				</div>
			</div>
			<div id="searchForm">
				<div className="filter">
				    <div className="row-flex">
						<RadioBox
							Title="Transfer Type"
							Options={initTransferType}
							OnChange={action.EditTransferTypeValue}
							dataColumn="transferType"
							Selected={state.transferType}
							Disabled={!isAdd}
						/>
					</div>
					{state.transferType == 4 ?
					<>
					<div className="row-flex">
						<RadioBox
							Title="Transaction Type"
							Options={initTransactionType}
							OnChange={action.EditMainValue}
							dataColumn="transactionType"
							Selected={state.transactionType}
							Disabled={!isAdd}
						/>
					</div>
					</>
					:
					<>
					</>
					}
					<div className="row-flex">
						<DateTimePickerElem
							columnTitle="Trans Date"
							dateTimePickerProps={{
								defaultValue: moment(state.transDate).format('YYYY-MM-DD'),
								timeFormat: false,
								onBlur: e => {
									try {
										action.EditMainValue(
											moment(e).format('YYYY-MM-DD'),
											"transDate"
										);
									} catch (ex) { }
								}
							}}
						/>
					</div>
					{state.transferType == 4 ?
					<>
						{state.transactionType == 1 ?
						<>
						<div className="panel level-1 mt-20">
							<div className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
								<span style={{ color: "white" }}>To Data</span>
							</div>
						</div>
						<div className="row-flex">
							<SelectBox
								Title="To Channel"
								Options={initChannelType}
								HandleChange={action.EditChannelSelectBox}
								dataColumn="toChannel"
								SelectedValue={state.toChannel}
							/>
						</div>
						<div className="row-flex">
							<SelectBox
								Title="To Site"
								Options={state.setToSiteList}
								HandleChange={action.EditSiteSelectBox}
								dataColumn="toSiteId"
								SelectedValue={state.toSiteId}
							/>
							{state.toSiteId == 0 || state.toSiteId == 4 ?
							<>
							</>
							:
							<>
								<InputBox
									Title="To CustId"
									OnChange={action.EditMainValue}
									dataColumn="toCustId"
									value={state.toCustId}
								/>
							{/* <InputBox
								Title="To UserName"
								OnChange={action.EditMainValue}
								dataColumn="toUserName"
								value={state.toUserName}
							/> */}
							</>
							}
						</div>
						<div className="row-flex">
							<SelectBox
								Title="To Currency"
								Options={state.setToCurrencyList}
								HandleChange={action.EditCurrencySelectBox}
								dataColumn="toCurrencyId"
								SelectedValue={state.toCurrencyId}
								mainKey={state.toSiteId}
							/>
							<SelectBox
								Title="To Bank Code"
								Options={state.setToBankCodeList}
								HandleChange={action.EditBankCodeSelectBox}
								dataColumn="toSysId"
								SelectedValue={state.toSysId}
							/> 
						</div>
						{state.toChannel == 0 ||
					 	state.toSiteId == 0 || 
					 	state.toCurrencyId == 0 ||
					 	state.toSysId == 0 ? 
					 	<></> 
					 	:
					 	<>
						<div className="row-flex">
					    	<SelectBox
								Title="To Account"
								Options={state.setToAccountList}
								HandleChange={action.EditAccountSelectBox}
								dataColumn="toAccount"
								SelectedValue={state.toAccount}
							/>
							{state.toChannel == 3 ?
							<>
								<SelectBox
									Title="To Pay"
									Options={state.payList}
									HandleChange={action.EditMainValue}
									dataColumn="toPayId"
									SelectedValue={state.toPayId}
									Disabled={true}
								/>
							</>
							:
							<>
								<InputBox
									Title="To Agent"
									OnChange={action.EditMainValue}
									dataColumn="toAgent"
									value={state.toAgent}
									Disabled={true}
								/>
							</>
							}
						</div>
						<div className="row-flex">
							<InputBox
								Title="To Account No."
								OnChange={action.EditMainValue}
								dataColumn="toAccountNo"
								value={state.toAccountNo}
								Disabled={true}
							/>
							<InputBox
								Title="To Account Display Name"
								OnChange={action.EditMainValue}
								dataColumn="toAccountDisplayName"
								value={state.toAccountDisplayName}
								Disabled={true}
							/>
						</div>
						<div className="row-flex">
							<InputBox
								Title="To Amount"
								OnChange={action.EditMainValue}
								dataColumn="toAmount"
								value={state.toAmount}
								type="number"
							/>
							<label>{number_format(state.toAmount)}</label>
						</div>
						<div className="row-flex">
							<InputBox
								Title="To Fee"
								OnChange={action.EditMainValue}
								dataColumn="toFee"
								value={state.toFee}
								type="number"
							/>
							<label>{number_format(state.toFee)}</label>
						</div>
						</>
                    	} 
					
					</>
						:
						<>
						</>
					}

					{state.transactionType == 2 ?
						<>
						<div className="panel level-1 mt-20">
						<div className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
							<span style={{ color: "white" }}>From Data</span>
						</div>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="From Channel"
							Options={initChannelType}
							HandleChange={action.EditChannelSelectBox}
							dataColumn="fromChannel"
							SelectedValue={state.fromChannel}
						/>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="From Site"
							Options={state.setFromSiteList}
							HandleChange={action.EditSiteSelectBox}
							dataColumn="fromSiteId"
							SelectedValue={state.fromSiteId}
						/>
						{state.fromSiteId == 0 || state.fromSiteId == 4 ?
						<>
						</>
						:
						<>
						<InputBox
							Title="From CustId"
							OnChange={action.EditMainValue}
							dataColumn="fromCustId"
							value={state.fromCustId}
						/>
						{/* <InputBox
							Title="From UserName"
							OnChange={action.EditMainValue}
							dataColumn="fromUserName"
							value={state.fromUserName}
						/> */}
						</>
						}
					</div>
					<div className="row-flex">
				
						<SelectBox
							Title="From Currency"
							Options={state.setFromCurrencyList}
							HandleChange={action.EditCurrencySelectBox}
							dataColumn="fromCurrencyId"
							SelectedValue={state.fromCurrencyId}
							mainKey={state.fromSiteId}
						/>
						<SelectBox
							Title="From Bank Code"
							Options={state.setFromBankCodeList}
							HandleChange={action.EditBankCodeSelectBox}
							dataColumn="fromSysId"
							SelectedValue={state.fromSysId}
						/> 
					</div>
					{state.fromChannel == 0 ||
					 state.fromSiteId == 0 || 
					 state.fromCurrencyId == 0 ||
					 state.fromSysId == 0 ? 
					 <></> 
					 :
					 <>
					<div className="row-flex">
					    <SelectBox
							Title="From Account"
							Options={state.setFromAccountList}
							HandleChange={action.EditAccountSelectBox}
							dataColumn="fromAccount"
							SelectedValue={state.fromAccount}
						/>
						
						{state.fromChannel == 3 ?
						<>
						<SelectBox
							Title="From Pay"
							Options={state.payList}
							HandleChange={action.EditMainValue}
							dataColumn="fromPayId"
							SelectedValue={state.fromPayId}
							Disabled={true}
						/>
						</>
						:
						<>
						<InputBox
							Title="From Agent"
							OnChange={action.EditMainValue}
							dataColumn="fromAgent"
							value={state.fromAgent}
							Disabled={true}
						/>
						</>
						}
					</div>
					<div className="row-flex">
					<InputBox
							Title="From Account No."
							OnChange={action.EditMainValue}
							dataColumn="fromAccountNo"
							value={state.fromAccountNo}
							Disabled={true}
						/>
						<InputBox
							Title="From Account Display Name"
							OnChange={action.EditMainValue}
							dataColumn="fromAccountDisplayName"
							value={state.fromAccountDisplayName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
					    <InputBox
							Title="From Amount"
							OnChange={action.EditMainValue}
							dataColumn="fromAmount"
							value={state.fromAmount}
							type="number"
						/>
						<label>{number_format(state.fromAmount)}</label>
					</div>
					<div className="row-flex">
					<InputBox
							Title="From Fee"
							OnChange={action.EditMainValue}
							dataColumn="fromFee"
							value={state.fromFee}
							type="number"
						/>
						<label>{number_format(state.fromFee)}</label>
					</div>
					</>}
						</>
						:
						<>
						</>
					}
						</>
						:
						<>
						</>
					}
					{state.transferType == 1 ||  state.transferType == 2 || state.transferType == 3 ?
						<>
						<div className="panel level-1 mt-20">
						<div className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
							<span style={{ color: "white" }}>From Data</span>
						</div>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="From Channel"
							Options={initChannelType}
							HandleChange={action.EditChannelSelectBox}
							dataColumn="fromChannel"
							SelectedValue={state.fromChannel}
						/>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="From Site"
							Options={state.setFromSiteList}
							HandleChange={action.EditSiteSelectBox}
							dataColumn="fromSiteId"
							SelectedValue={state.fromSiteId}
						/>
						{state.fromSiteId == 0 || state.fromSiteId == 4 || state.transferType == 2?
						<>
						</>
						:
						<>
						<InputBox
							Title="From CustId"
							OnChange={action.EditMainValue}
							dataColumn="fromCustId"
							value={state.fromCustId}
						/>
						{/* <InputBox
							Title="From UserName"
							OnChange={action.EditMainValue}
							dataColumn="fromUserName"
							value={state.fromUserName}
						/> */}
						</>
						}
					</div>
					<div className="row-flex">
				
						<SelectBox
							Title="From Currency"
							Options={state.setFromCurrencyList}
							HandleChange={action.EditCurrencySelectBox}
							dataColumn="fromCurrencyId"
							SelectedValue={state.fromCurrencyId}
							mainKey={state.fromSiteId}
						/>
						<SelectBox
							Title="From Bank Code"
							Options={state.setFromBankCodeList}
							HandleChange={action.EditBankCodeSelectBox}
							dataColumn="fromSysId"
							SelectedValue={state.fromSysId}
						/>
						</div>
						{state.fromChannel == 0 ||
					 state.fromSiteId == 0 || 
					 state.fromCurrencyId == 0 ||
					 state.fromSysId == 0 ? 
					 <></> 
					 :
					 <> 
					<div className="row-flex">
						{state.transferType == 1 ? <></> :
						 <>
						 <SelectBox
						 Title="From Account"
						 Options={state.setFromAccountList}
						 HandleChange={action.EditAccountSelectBox}
						 dataColumn="fromAccount"
						 SelectedValue={state.fromAccount}
					 />
					 {state.fromChannel == 3 ?
						<>
						{((state.transferType == 2 || state.transferType ==3) && state.fromChannel == 3) ?
						<SelectBox
						Title="From Pay"
						Options={state.payList}
						HandleChange={action.EditMainValue}
						dataColumn="fromPayId"
						SelectedValue={state.fromPayId}
						Disabled={true}
					/>:
					<SelectBox
							Title="From Pay"
							Options={state.payList}
							HandleChange={action.EditMainValue}
							dataColumn="fromPayId"
							SelectedValue={state.fromPayId}
						/>
						}
						</>
						:
						<>
						{((state.transferType == 2 || state.transferType ==3) && state.fromChannel != 3) ?
						<InputBox
						Title="From Agent"
						OnChange={action.EditMainValue}
						dataColumn="fromAgent"
						value={state.fromAgent}
						Disabled={true}
					/>:
					<InputBox
							Title="From Agent"
							OnChange={action.EditMainValue}
							dataColumn="fromAgent"
							value={state.fromAgent}
						/>
						}
						</>
						} 
					 </>
					 }
					</div>
					<div className="row-flex">
						{(state.transferType==2 || state.transferType==3) ?<><InputBox
							Title="From Account No."
							OnChange={action.EditMainValue}
							dataColumn="fromAccountNo"
							value={state.fromAccountNo}
							Disabled={true}
						/>
						<InputBox
							Title="From Account Display Name"
							OnChange={action.EditMainValue}
							dataColumn="fromAccountDisplayName"
							value={state.fromAccountDisplayName}
							Disabled={true}
						/></>: <>
						<InputBox
						Title="From Account No."
						OnChange={action.EditMainValue}
						dataColumn="fromAccountNo"
						value={state.fromAccountNo}
					/>
					<InputBox
						Title="From Account Display Name"
						OnChange={action.EditMainValue}
						dataColumn="fromAccountDisplayName"
						value={state.fromAccountDisplayName}
					/></>}
					</div>
					<div className="row-flex">
					    <InputBox
							Title="From Amount"
							OnChange={action.EditMainValue}
							dataColumn="fromAmount"
							value={state.fromAmount}
							type="number"
						/>
						<label>{number_format(state.fromAmount)}</label>
					</div>
					<div className="row-flex">
					<InputBox
							Title="From Fee"
							OnChange={action.EditMainValue}
							dataColumn="fromFee"
							value={state.fromFee}
							type="number"
						/>
						<label>{number_format(state.fromFee)}</label>
					</div>
					</>
					}
					<div className="panel level-1 mt-20">
						<div className="panel-title no-arrow" style={{ justifyContent: "space-between" }}>
							<span style={{ color: "white" }}>To Data</span>
						</div>
					</div>
					<div className="row-flex">
						<SelectBox
							Title="To Channel"
							Options={initChannelType}
							HandleChange={action.EditChannelSelectBox}
							dataColumn="toChannel"
							SelectedValue={state.toChannel}
						/>
						
					</div>
					<div className="row-flex">
						<SelectBox
							Title="To Site"
							Options={state.setToSiteList}
							HandleChange={action.EditSiteSelectBox}
							dataColumn="toSiteId"
							SelectedValue={state.toSiteId}
						/>
						{state.toSiteId == 0 || state.toSiteId == 4 || state.transferType == 1 || state.transferType == 3 ?
						<>
						</>
						:
						<>
						<InputBox
							Title="To CustId"
							OnChange={action.EditMainValue}
							dataColumn="toCustId"
							value={state.toCustId}
						/>
						{/* <InputBox
							Title="To UserName"
							OnChange={action.EditMainValue}
							dataColumn="toUserName"
							value={state.toUserName}
						/> */}
						</>
						}
					</div>
					<div className="row-flex">
				
						<SelectBox
							Title="To Currency"
							Options={state.setToCurrencyList}
							HandleChange={action.EditCurrencySelectBox}
							dataColumn="toCurrencyId"
							SelectedValue={state.toCurrencyId}
							mainKey={state.toSiteId}
						/>
						<SelectBox
							Title="To Bank Code"
							Options={state.setToBankCodeList}
							HandleChange={action.EditBankCodeSelectBox}
							dataColumn="toSysId"
							SelectedValue={state.toSysId}
						/> 
					</div>
					{state.toChannel == 0 ||
					 state.toSiteId == 0 || 
					 state.toCurrencyId == 0 ||
					 state.toSysId == 0 ? 
					 <></> 
					 :
					 <> 
					<div className="row-flex">
					{state.transferType==2 ? <></> : <SelectBox
							Title="To Account"
							Options={state.setToAccountList}
							HandleChange={action.EditAccountSelectBox}
							dataColumn="toAccount"
							SelectedValue={state.toAccount}
						/>}
						{((state.transferType == 1 || state.transferType ==3) && state.fromChannel == 3) ?
						<>
						<SelectBox
							Title="To Pay"
							Options={state.payList}
							HandleChange={action.EditMainValue}
							dataColumn="toPayId"
							SelectedValue={state.toPayId}
							Disabled={true}
						/>
						</>
						:
						<>
						</>
                        }
{((state.transferType == 1 || state.transferType ==3) && state.fromChannel != 3) ?
						<>
						<InputBox
							Title="To Agent"
							OnChange={action.EditMainValue}
							dataColumn="toAgent"
							value={state.toAgent}
							Disabled={true}
						/>
						</>
						:<></>
						}
					</div>
					<div className="row-flex">
						{state.transferType==1 || state.transferType==3 ?<>
						<InputBox
							Title="To Account No."
							OnChange={action.EditMainValue}
							dataColumn="toAccountNo"
							value={state.toAccountNo}
							Disabled={true}
						/>
						<InputBox
							Title="To Account Display Name"
							OnChange={action.EditMainValue}
							dataColumn="toAccountDisplayName"
							value={state.toAccountDisplayName}
							Disabled={true}
						/>
						</> : 
						<>
						<InputBox
							Title="To Account No."
							OnChange={action.EditMainValue}
							dataColumn="toAccountNo"
							value={state.toAccountNo}
						/>
						<InputBox
							Title="To Account Display Name"
							OnChange={action.EditMainValue}
							dataColumn="toAccountDisplayName"
							value={state.toAccountDisplayName}
						/>
						</>}
					</div>
					<div className="row-flex">
						<InputBox
							Title="To Amount"
							OnChange={action.EditMainValue}
							dataColumn="toAmount"
							value={state.toAmount}
							type="number"
						/>
						<label>{number_format(state.toAmount)}</label>
					</div>
					<div className="row-flex">
						<InputBox
							Title="To Fee"
							OnChange={action.EditMainValue}
							dataColumn="toFee"
							value={state.toFee}
							type="number"
						/>
						<label>{number_format(state.toFee)}</label>
					</div>
					</> }
						</>
						:
						<>
						</>
						}
					<hr></hr>
					<div className="row-flex">
						<SelectBox
							Title="Reason"
							Options={state.reasonList}
							HandleChange={action.EditMainValue}
							dataColumn="reason"
							SelectedValue={state.reason}
						/>
					</div>
					<div className="row-flex">
						<TextBox
							Title="Remark"
							OnChange={action.EditMainValue}
							dataColumn="remark"
							value={state.remark}
						/>
					</div>
				   <div className="row-flex"> 
				   <UploadBox
				            Id="UploadReceipt"
							Title="Upload Receipt"
							type="file"
							Accept=".png, .jpg, .jpeg"
							OnChange={action.UploadReceipt}
							dataColumn="uploadReceipt"
							value={state.uploadReceipt}
						/>
						<label>{state.uploadReceiptName}</label>
                   </div>
				   { state.transferPic==null || state.transferPic==undefined ?
				   <></>
				   :
				   <>
                 <img className="img-receipt" src={`data:image/jpeg;base64,${state.transferPic}`} /> 
				 </>
				   }
					<hr></hr>
					<div className="row-flex">
						<InputBox
							Title="Create Time"
							OnChange={action.EditMainValue}
							dataColumn="createTime"
							value={moment.parseZone(state.createTime).format("YYYY-MM-DD HH:mm:ss")}
							Disabled={true}
						/>
						<InputBox
							Title="Creator"
							OnChange={action.EditMainValue}
							dataColumn="creatorName"
							value={state.creatorName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="Update Time"
							OnChange={action.EditMainValue}
							dataColumn="updateTime"
							value={moment.parseZone(state.updateTime).format("YYYY-MM-DD HH:mm:ss")}
							Disabled={true}
						/>
						<InputBox
							Title="Updator"
							OnChange={action.EditMainValue}
							dataColumn="updatorName"
							value={state.updatorName}
							Disabled={true}
						/>
					</div>
					<div className="row-flex">
						<InputBox
							Title="TransferId"
							OnChange={action.EditMainValue}
							dataColumn="transferId"
							value={state.transferId}
							Disabled={true}
						/>
					</div>
				</div>
			</div>
			<div className="filter float-btn-group">
				<button
					className="btn btn-back"
					onClick={action.ClickBack}
					disabled={processing}>
					Back
				</button>
				<button
					className="btn btn-add"
					onClick={action.ClickSubmit}
					disabled={processing}>
					Submit
				</button>
			</div>
		</div>
	);
};
